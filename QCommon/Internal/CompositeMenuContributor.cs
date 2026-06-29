using System.Collections.Immutable;
using LoupixDeck.PluginSdk;
using Microsoft.Extensions.DependencyInjection;

namespace QCommon.Internal;

internal sealed class CompositeMenuContributor(ImmutableArray<MenuContributorBase> contributors) : IMenuContributor
{
    [ActivatorUtilitiesConstructor]
    public CompositeMenuContributor(IEnumerable<MenuContributorBase> contributors) : this(contributors.ToImmutableArray()) { }
    public Task<IReadOnlyList<MenuNode>> GetMenuNodes(ButtonTargets target)
    {
        ImmutableList<MenuNode>.Builder nodesBuilder = ImmutableList.CreateBuilder<MenuNode>();
        List<Task<ImmutableList<MenuNode>>> tasks = new();
        List<Exception>? exs = null;
        foreach (var contributor in contributors)
        {
            try
            {
                ValueTask<ImmutableList<MenuNode>> task = contributor.GetMenuNodes(target);
                if (task.IsCompletedSuccessfully)
                    nodesBuilder.AddRange(task.GetAwaiter().GetResult());
                else
                    tasks.Add(task.AsTask());
            }
            catch (Exception ex)
            {
                (exs ??= new()).Add(ex);
            }
        }

        if (tasks.Count is not 0)
            return AsyncTail(nodesBuilder, tasks.ToArray(), exs);
        else if (exs is not null)
            throw new AggregateException(exs);
        else
            return Task.FromResult<IReadOnlyList<MenuNode>>(nodesBuilder.ToImmutable());
    }

    private static async Task<IReadOnlyList<MenuNode>> AsyncTail(ImmutableList<MenuNode>.Builder nodes, Task<ImmutableList<MenuNode>>[] tasks, List<Exception>? exs)
    {
        await foreach (var task in Task.WhenEach(tasks))
        {
            ImmutableList<MenuNode> taskRes;
            try
            {
                taskRes = await task;
            }
            catch (Exception ex)
            {
                (exs ??= new()).Add(ex);
                continue;
            }
            nodes.AddRange(taskRes);
        }
        if (exs is not null)
            throw new AggregateException(exs);
        return nodes.ToImmutable();
    }
}
