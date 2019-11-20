/*jslint plusplus: true */
/*jslint nomen: true */

// JScript File
var ProperCase = (function () {
    'use strict';
    //split a string s with the delimeter del
    function split(s, del) {
        var arrS = [],
            i = 0,
            j = 0,
            k = 0,
            delim = String(del);

        //Is the delimeter in the string
        if (s.indexOf(delim) !== -1) {
            for (i = 0; i < s.length; i++) {
                if (s.charAt(i) === delim) {
                    if (k === 0) {
                        arrS[j] = s.substring(k, i);
                    } else {
                        arrS[j] = s.substring(k + 1, i);
                    }

                    k = i;
                    j++;
                }
            }

            arrS[j] = s.substring(k + 1, s.length);
        } else {
            arrS[0] = s;
        }

        return arrS;
    }

    function _isUpper(c) {
        return (c.charCodeAt(0) >= 65) && (c.charCodeAt(0) <= 90);
    }

    function _isLower(c) {
        return (c.charCodeAt(0) >= 97) && (c.charCodeAt(0) <= 122);
    }


    // A special case word is one such as FirstAgain where the first letter is upper, and a middle letter is also upper.
    // These are assumed to be in correct proper case.
    function _isSpecialCase(string) {
        var i;

        if (_isUpper(string.charAt(0))      // first char is upper
                && _isLower(string.charAt(1))) { // second char is lower
            for (i = 2; i < string.length; i++) {
                if (_isUpper(string.charAt(i))) { //middle char is upper
                    return true;
                }
            }
        }

        return false;
    }

    function _pCase(STRING) {
        var UcaseNext = false,
            SkipNext = false,
            iCounter,
            iChar,
            strReturn_Value = "",
            iTemp = STRING.length;

        if (iTemp === 0) {
            return "";
        }

        strReturn_Value += STRING.charAt(0).toUpperCase();
        for (iCounter = 1; iCounter < iTemp; iCounter++) {
            if (UcaseNext === true) { // make upper case
                strReturn_Value += STRING.charAt(iCounter).toUpperCase();
            } else if (SkipNext === true) { // leave it as the user entered it.
                strReturn_Value += STRING.charAt(iCounter);
            } else { // make lowercase
                strReturn_Value += STRING.charAt(iCounter).toLowerCase();
            }

            iChar = STRING.charCodeAt(iCounter);

            UcaseNext = false;
            SkipNext = false;

            //uppercase in middle of word after these chars
            if (iChar === 32     // space
                    || iChar === 45  // dash
                    || iChar === 46) { // period
                UcaseNext = true;
            } else if (iChar === 39) { // apostrophe
                SkipNext = true;
            } else {
                UcaseNext = false;
            }

            // special name case for Mc
            if (iChar === 99 || iChar === 67) { //upper or lower c
                if (STRING.charCodeAt(iCounter - 1) === 77 || STRING.charCodeAt(iCounter - 1) === 109) { // upper or lower m
                    UcaseNext = true;
                }
            }

        } //End For

        return strReturn_Value;

    } //End Function

    function _properCase(string) {
        var i,
            j,
            thisWord,
            skip,
            tokens = [],
            ret = "",
            // the following skipTokens should all be uppercase. if they are found as single words within the string,
            // they will be converted to all uppercase if they are not already.
            skipTokens = ['II', 'II.', 'III', 'III.', 'IV', 'IV.', 'V', 'V.', 'PO', 'PO.', 'APO', 'FPO'];

        tokens = split(string, ' ');

        for (i = 0; i < tokens.length; i++) {
            thisWord = tokens[i];
            skip = false;

            for (j = 0; j < skipTokens.length; j++) {
                if (thisWord.toUpperCase() === skipTokens[j]) {
                    thisWord = thisWord.toUpperCase();
                    skip = true;
                }
            }

            if (_isSpecialCase(thisWord)) {
                skip = true;
            }

            if (!skip) {
                ret += _pCase(thisWord);
            } else {
                ret += thisWord;
            }

            if (tokens.length !== i + 1) {
                ret += ' ';
            }
        }

        return ret;
    }

    return {
        transform: _properCase
    };
}());

