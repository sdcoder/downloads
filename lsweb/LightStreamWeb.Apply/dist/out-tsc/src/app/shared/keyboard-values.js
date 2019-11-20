var KeyBoardValues = /** @class */ (function () {
    function KeyBoardValues() {
    }
    KeyBoardValues.navigationKeyCodes = [
        8,
        9,
        35,
        36,
        37,
        38,
        39,
        40,
        45,
        46,
        112,
        113,
        114,
        115,
        116,
        118,
        119,
        120,
        121,
        122,
        123,
        229
    ];
    KeyBoardValues.editKeyCodes = [8, 9, 35, 36, 37, 38, 39, 40, 45];
    KeyBoardValues.numericKeyCodes = [
        48,
        49,
        50,
        51,
        52,
        53,
        54,
        55,
        56,
        57,
        96,
        97,
        98,
        99,
        100,
        101,
        102,
        103,
        104,
        105
    ];
    KeyBoardValues.decimalKeyCodes = KeyBoardValues.numericKeyCodes.concat(190, 110);
    KeyBoardValues.navigationKeys = [
        'Backspace',
        'Tab',
        'End',
        'Home',
        'ArrowLeft',
        'Up',
        'ArrowRight',
        'Down',
        'Insert',
        'Del',
        'F1',
        'F2',
        'F3',
        'F4',
        'F5',
        'F6',
        'F7',
        'F8',
        'F9',
        'F10',
        'F11',
        'F12',
    ];
    KeyBoardValues.parenthesis = [
        '(',
        ')'
    ];
    KeyBoardValues.hyphen = '-';
    return KeyBoardValues;
}());
export { KeyBoardValues };
//# sourceMappingURL=keyboard-values.js.map