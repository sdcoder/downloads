namespace LightStreamWeb.Frontend {
    class SessionTimeoutService {
        private _sessionTimeoutWarning: JQuery;
        private _sessionTimeoutSeconds: JQuery;
        private _showDialogTimer: number;
        private _secondsLeft: number;
        private _secondsLeftTimer: number;
        private _timerStartTime: number;

        private _getIsUserLoggedIn(): boolean {
            // If the 'sign out' link is on the page, then set a timer to warn the user their session will expire soon.
            return $('.navbar-container .navbar-navigation-header-actions .sign-out-action').length > 0;
        }

        private _getSessionTimeout(): number {
            if (window.location.href.indexOf("timeouttest") > -1) return 1000 * 10;
            else return 1000 * 60 * 8; // 8 minutes - one less than forms authentication timout, two less than session timeout
        }

        constructor() {
            let that = this;

            this._sessionTimeoutWarning = $('#TimeoutModal');
            this._sessionTimeoutSeconds = $('#Countdown');

            this._sessionTimeoutWarning.on("closed.zf.reveal", async function () {
                clearInterval(that._secondsLeftTimer);
                that._secondsLeftTimer = 0;
                that.startTimer();

                await $.ajax({
                    url: '/SignIn/Extend',
                    cache: false
                });
            });

            // Verify that the browser supports the visibility API.
            if (typeof document.addEventListener !== 'undefined' && typeof document.hidden !== 'undefined' && typeof window.sessionStorage !== 'undefined')
                document.addEventListener('visibilitychange', this._visibilityStateChanged.bind(this), false);
        }

        private _getTimeString(secondsLeft: number): string {
            // We want the integer part of the number for the minutes.
            let minutes = Math.floor(secondsLeft / 60);
            // We want the remainder for the seconds.
            let seconds = secondsLeft % 60;

            return `${minutes}:${seconds > 9 ? seconds : `0${seconds}`}`;
        }

        private _showDialogTimerTick(secondsLeft: number): void {
            // Only open the dialog if not already open.
            if (!(this._sessionTimeoutWarning.parent().css('display') == 'block'))
                this._sessionTimeoutWarning.foundation('open');
            this._secondsLeft = secondsLeft;
            // One warning timer at a time.
            if (!this._secondsLeftTimer)
                this._secondsLeftTimer = setInterval(this._warningTimerTick.bind(this), 1000) as any // node types differ from typescript types and thus this cast;
            // Update the gui, but do not reduce the counter.
            this._warningTimerTick(false);
        }

        private _signOut(): void {
            clearInterval(this._secondsLeftTimer);
            this._secondsLeftTimer = 0;
            // Sign the user out.
            window.location.href = '/signin/logout?timeout=true&timer=expired&status=timeout';
        }

        public startTimer(remainingTime?: number): void {
            // Set the remaining time to default if not provided.
            if (typeof remainingTime === 'undefined') remainingTime = this._getSessionTimeout();

            // Keep track of when we started the timer.  Will be used later if the user navigates away.
            this._timerStartTime = Date.now();

            this._showDialogTimer = setTimeout(this._showDialogTimerTick.bind(this, [120]), remainingTime) as any // node types differ from typescript types and thus this cast;
        }

        private _visibilityStateChanged() {
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
                let visibilityHiddenInfo: {
                    timerStartTime: number
                } = JSON.parse(window.sessionStorage.getItem('signin.visibilityStateChangedTimer') || '{}');
                window.sessionStorage.removeItem('signin.visibilityStateChangedTimer');

                if (visibilityHiddenInfo.timerStartTime && !this._secondsLeftTimer) {
                    let dateDifference = Date.now() - visibilityHiddenInfo.timerStartTime;
                    let sessionTimeout = this._getSessionTimeout();

                    if (dateDifference < sessionTimeout) this.startTimer(sessionTimeout - dateDifference); // Start the timer with the remaining time.
                    else if (dateDifference - sessionTimeout < 120 * 1000) this._showDialogTimerTick(120 - Math.floor((dateDifference - sessionTimeout) / 1000)); // Display the remaining dialog time.
                    else this._signOut(); // The user has missed the dialog.  Log them out.
                }
            }
        }

        private _warningTimerTick(decrementTime: boolean = true) {
            if (decrementTime) this._secondsLeft--;
            if (this._secondsLeft <= 0) this._signOut();

            // Update the UI.
            this._sessionTimeoutSeconds.html(this._getTimeString(this._secondsLeft));
        }
    }

    $(function () {
        let sessionTimeoutService = new SessionTimeoutService();
        sessionTimeoutService.startTimer();
    });
}