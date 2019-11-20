var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var LightStreamWeb;
(function (LightStreamWeb) {
    var Frontend;
    (function (Frontend) {
        var SessionTimeoutService = /** @class */ (function () {
            function SessionTimeoutService() {
                var that = this;
                this._sessionTimeoutWarning = $('#TimeoutModal');
                this._sessionTimeoutSeconds = $('#Countdown');
                this._sessionTimeoutWarning.on("closed.zf.reveal", function () {
                    return __awaiter(this, void 0, void 0, function () {
                        return __generator(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    clearInterval(that._secondsLeftTimer);
                                    that._secondsLeftTimer = 0;
                                    that.startTimer();
                                    return [4 /*yield*/, $.ajax({
                                            url: '/SignIn/Extend',
                                            cache: false
                                        })];
                                case 1:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    });
                });
                // Verify that the browser supports the visibility API.
                if (typeof document.addEventListener !== 'undefined' && typeof document.hidden !== 'undefined' && typeof window.sessionStorage !== 'undefined')
                    document.addEventListener('visibilitychange', this._visibilityStateChanged.bind(this), false);
            }
            SessionTimeoutService.prototype._getIsUserLoggedIn = function () {
                // If the 'sign out' link is on the page, then set a timer to warn the user their session will expire soon.
                return $('.navbar-container .navbar-navigation-header-actions .sign-out-action').length > 0;
            };
            SessionTimeoutService.prototype._getSessionTimeout = function () {
                if (window.location.href.indexOf("timeouttest") > -1)
                    return 1000 * 10;
                else
                    return 1000 * 60 * 8; // 8 minutes - one less than forms authentication timout, two less than session timeout
            };
            SessionTimeoutService.prototype._getTimeString = function (secondsLeft) {
                // We want the integer part of the number for the minutes.
                var minutes = Math.floor(secondsLeft / 60);
                // We want the remainder for the seconds.
                var seconds = secondsLeft % 60;
                return minutes + ":" + (seconds > 9 ? seconds : "0" + seconds);
            };
            SessionTimeoutService.prototype._showDialogTimerTick = function (secondsLeft) {
                // Only open the dialog if not already open.
                if (!(this._sessionTimeoutWarning.parent().css('display') == 'block'))
                    this._sessionTimeoutWarning.foundation('open');
                this._secondsLeft = secondsLeft;
                // One warning timer at a time.
                if (!this._secondsLeftTimer)
                    this._secondsLeftTimer = setInterval(this._warningTimerTick.bind(this), 1000); // node types differ from typescript types and thus this cast;
                // Update the gui, but do not reduce the counter.
                this._warningTimerTick(false);
            };
            SessionTimeoutService.prototype._signOut = function () {
                clearInterval(this._secondsLeftTimer);
                this._secondsLeftTimer = 0;
                // Sign the user out.
                window.location.href = '/signin/logout?timeout=true&timer=expired&status=timeout';
            };
            SessionTimeoutService.prototype.startTimer = function (remainingTime) {
                // Set the remaining time to default if not provided.
                if (typeof remainingTime === 'undefined')
                    remainingTime = this._getSessionTimeout();
                // Keep track of when we started the timer.  Will be used later if the user navigates away.
                this._timerStartTime = Date.now();
                this._showDialogTimer = setTimeout(this._showDialogTimerTick.bind(this, [120]), remainingTime); // node types differ from typescript types and thus this cast;
            };
            SessionTimeoutService.prototype._visibilityStateChanged = function () {
                // Save the date so we can do some math when we get back.
                if (document.visibilityState !== 'visible') {
                    window.sessionStorage.setItem('signin.visibilityStateChangedTimer', JSON.stringify({
                        timerStartTime: this._timerStartTime
                    }));
                    clearTimeout(this._showDialogTimer);
                    clearInterval(this._secondsLeftTimer);
                    this._secondsLeftTimer = 0;
                }
                else {
                    var visibilityHiddenInfo = JSON.parse(window.sessionStorage.getItem('signin.visibilityStateChangedTimer') || '{}');
                    window.sessionStorage.removeItem('signin.visibilityStateChangedTimer');
                    if (visibilityHiddenInfo.timerStartTime && !this._secondsLeftTimer) {
                        var dateDifference = Date.now() - visibilityHiddenInfo.timerStartTime;
                        var sessionTimeout = this._getSessionTimeout();
                        if (dateDifference < sessionTimeout)
                            this.startTimer(sessionTimeout - dateDifference); // Start the timer with the remaining time.
                        else if (dateDifference - sessionTimeout < 120 * 1000)
                            this._showDialogTimerTick(120 - Math.floor((dateDifference - sessionTimeout) / 1000)); // Display the remaining dialog time.
                        else
                            this._signOut(); // The user has missed the dialog.  Log them out.
                    }
                }
            };
            SessionTimeoutService.prototype._warningTimerTick = function (decrementTime) {
                if (decrementTime === void 0) { decrementTime = true; }
                if (decrementTime)
                    this._secondsLeft--;
                if (this._secondsLeft <= 0)
                    this._signOut();
                // Update the UI.
                this._sessionTimeoutSeconds.html(this._getTimeString(this._secondsLeft));
            };
            return SessionTimeoutService;
        }());
        $(function () {
            var sessionTimeoutService = new SessionTimeoutService();
            sessionTimeoutService.startTimer();
        });
    })(Frontend = LightStreamWeb.Frontend || (LightStreamWeb.Frontend = {}));
})(LightStreamWeb || (LightStreamWeb = {}));
//# sourceMappingURL=SessionTimeoutService.js.map