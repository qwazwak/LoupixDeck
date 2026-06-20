using LoupixDeck.Models;

namespace LoupixDeck.Utils;

public static class KeyboardLayouts
{
    private static KeyboardLayout Us { get; } = new KeyboardLayout("us",
    [
        new(' ', (57, false)),
        new('!', (2, true)),
        new('"', (40, true)),
        new('#', (4, true)),
        new('$', (5, true)),
        new('%', (6, true)),
        new('&', (8, true)),
        new('\'', (40, false)),
        new('(', (10, true)),
        new(')', (11, true)),
        new('*', (9, true)),
        new('+', (13, true)),
        new(',', (51, false)),
        new('-', (12, false)),
        new('.', (52, false)),
        new('/', (53, false)),
        new('0', (11, false)),
        new('1', (2, false)),
        new('2', (3, false)),
        new('3', (4, false)),
        new('4', (5, false)),
        new('5', (6, false)),
        new('6', (7, false)),
        new('7', (8, false)),
        new('8', (9, false)),
        new('9', (10, false)),
        new(':', (39, true)),
        new(';', (39, false)),
        new('<', (51, true)),
        new('=', (13, false)),
        new('>', (52, true)),
        new('?', (53, true)),
        new('@', (3, true)),
        new('A', (30, true)),
        new('B', (48, true)),
        new('C', (46, true)),
        new('D', (32, true)),
        new('E', (18, true)),
        new('F', (33, true)),
        new('G', (34, true)),
        new('H', (35, true)),
        new('I', (23, true)),
        new('J', (36, true)),
        new('K', (37, true)),
        new('L', (38, true)),
        new('M', (50, true)),
        new('N', (49, true)),
        new('O', (24, true)),
        new('P', (25, true)),
        new('Q', (16, true)),
        new('R', (19, true)),
        new('S', (31, true)),
        new('T', (20, true)),
        new('U', (22, true)),
        new('V', (47, true)),
        new('W', (17, true)),
        new('X', (45, true)),
        new('Y', (21, true)),
        new('Z', (44, true)),
        new('[', (26, false)),
        new('\\', (43, false)),
        new(']', (27, false)),
        new('^', (7, true)),
        new('_', (12, true)),
        new('`', (41, false)),
        new('a', (30, false)),
        new('b', (48, false)),
        new('c', (46, false)),
        new('d', (32, false)),
        new('e', (18, false)),
        new('f', (33, false)),
        new('g', (34, false)),
        new('h', (35, false)),
        new('i', (23, false)),
        new('j', (36, false)),
        new('k', (37, false)),
        new('l', (38, false)),
        new('m', (50, false)),
        new('n', (49, false)),
        new('o', (24, false)),
        new('p', (25, false)),
        new('q', (16, false)),
        new('r', (19, false)),
        new('s', (31, false)),
        new('t', (20, false)),
        new('u', (22, false)),
        new('v', (47, false)),
        new('w', (17, false)),
        new('x', (45, false)),
        new('y', (21, false)),
        new('z', (44, false)),
        new('{', (26, true)),
        new('|', (43, true)),
        new('}', (27, true)),
        new('~', (41, true)),
    ]);

    private static KeyboardLayout De { get; } = new KeyboardLayout("de",
    [
        new(' ', (57, false)),

        new('1', (2, false)),
        new('!', (2, true)),

        new('2', (3, false)),
        new('"', (3, true)),

        new('3', (4, false)),
        new('§', (4, true)),

        new('4', (5, false)),
        new('$', (5, true)),

        new('5', (6, false)),
        new('%', (6, true)),

        new('6', (7, false)),
        new('&', (7, true)),

        new('7', (8, false)),
        new('/', (8, true)),

        new('8', (9, false)),
        new('(', (9, true)),

        new('9', (10, false)),
        new(')', (10, true)),

        new('0', (11, false)),
        new('=', (11, true)),

        new('ß', (12, false)),
        new('?', (12, true)),

        new('´', (13, false)),
        new('`', (13, true)),

        new('q', (16, false)),
        new('Q', (16, true)),

        new('w', (17, false)),
        new('W', (17, true)),

        new('e', (18, false)),
        new('E', (18, true)),

        new('r', (19, false)),
        new('R', (19, true)),

        new('t', (20, false)),
        new('T', (20, true)),

        new('z', (21, false)),
        new('Z', (21, true)),

        new('u', (22, false)),
        new('U', (22, true)),

        new('i', (23, false)),
        new('I', (23, true)),

        new('o', (24, false)),
        new('O', (24, true)),

        new('p', (25, false)),
        new('P', (25, true)),

        new('ü', (26, false)),
        new('Ü', (26, true)),

        new('+', (27, false)),
        new('*', (27, true)),

        new('a', (30, false)),
        new('A', (30, true)),

        new('s', (31, false)),
        new('S', (31, true)),

        new('d', (32, false)),
        new('D', (32, true)),

        new('f', (33, false)),
        new('F', (33, true)),

        new('g', (34, false)),
        new('G', (34, true)),

        new('h', (35, false)),
        new('H', (35, true)),

        new('j', (36, false)),
        new('J', (36, true)),

        new('k', (37, false)),
        new('K', (37, true)),

        new('l', (38, false)),
        new('L', (38, true)),

        new('ö', (39, false)),
        new('Ö', (39, true)),

        new('ä', (40, false)),
        new('Ä', (40, true)),

        new('#', (43, false)),
        new('\'', (43, true)),

        new('y', (44, false)),
        new('Y', (44, true)),

        new('x', (45, false)),
        new('X', (45, true)),

        new('c', (46, false)),
        new('C', (46, true)),

        new('v', (47, false)),
        new('V', (47, true)),

        new('b', (48, false)),
        new('B', (48, true)),

        new('n', (49, false)),
        new('N', (49, true)),

        new('m', (50, false)),
        new('M', (50, true)),

        new(',', (51, false)),
        new(';', (51, true)),

        new('.', (52, false)),
        new(':', (52, true)),

        new('-', (53, false)),
        new('_', (53, true)),
    ]);

    public static KeyboardLayout GetLayout(string locale)
    {
        return locale.ToLower() switch
        {
            "us" => Us,
            "de" => De,
            _ => Us
        };
    }
}