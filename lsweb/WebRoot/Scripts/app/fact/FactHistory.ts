namespace LightStreamWeb.Frontend {
    let lightstreamAppModule = angular.module('ls.services');
    let $ = jQuery;

    export interface FactModel {
        ID: number,
        Timestamp: Date,
        ExpirationDate: Date
    }

    interface ServerTimeInfo {
        now: Date,
        expirationDate: Date
    }

    function addDays(date: Date, days: number) {
        let result = new Date(date.valueOf());
        result.setDate(result.getDate() + days);
        return result;
    }

    export interface IFactHistory {
        addFactIdAsync(fact: number): Promise<void>;
        getFactIdsRemovingExpiredAsync(serverTimeInfo?: ServerTimeInfo): Promise<FactModel[]>;
        clearHistory(): void;
    }

    class FactHistory implements IFactHistory {
        private readonly FACT_HISTORY_KEY = "FactHistory";
        private _isLocalStorageSupported: boolean;

        constructor(
            private $http: angular.IHttpService,
            private $window: angular.IWindowService
        ) {
            this._isLocalStorageSupported = this._supportsLocalStorage();
        }

        private _isFactIDTopOfList(fact: number, factHistory: FactModel[]): boolean {
            if (factHistory.length === 0) return false;

            let last = factHistory[factHistory.length - 1];

            return last && last.ID === fact;
        }

        private _getItem(key: string): string {
            if (this._isLocalStorageSupported) return localStorage.getItem(key);
            else return null;
        }

        private _getServerTimeInfoAsync(): Promise<angular.IHttpPromiseCallbackArg<ServerTimeInfo>> {
            return this.$http.get('/api/servertimeinfo', {
                transformResponse: function (data, headersGetter): ServerTimeInfo {
                    let serverData = JSON.parse(data);
                    let currentDate = new Date(serverData.Now);

                    return {
                        now: currentDate,
                        expirationDate: addDays(currentDate, serverData.ExpirationTime)
                    };
                }
            }) as any;  // node types differ from typescript types and thus this cast
        }

        private _setItem(key: string, data: string): void {
            if (this._isLocalStorageSupported) localStorage.setItem(key, data);
        }

        private _supportsLocalStorage(): boolean {
            // Handle local storage not exisiting.
            if (typeof window.localStorage === 'undefined') return false;

            // Handle Safari in private mode.
            try {
                localStorage.setItem('test', 'testValue');
                localStorage.removeItem('test');
                return true;
            } catch (e) {
                return false;
            }
        }

        public async addFactIdAsync(fact: number): Promise<void> {
            let serverTimeInfo = (await this._getServerTimeInfoAsync()).data;

            let factHistory = await this.getFactIdsRemovingExpiredAsync(serverTimeInfo);

            // We don't want to add the fact if it was the last one we inserted.
            if (!this._isFactIDTopOfList(fact, factHistory)) {
                factHistory.push({
                    ID: fact,
                    Timestamp: serverTimeInfo.now,
                    ExpirationDate: serverTimeInfo.expirationDate
                });

                // Remove from end.
                if (factHistory.length > 10) factHistory.splice(0, 1);

                localStorage.setItem(this.FACT_HISTORY_KEY, JSON.stringify(factHistory));
            }
        }

        public clearHistory(): void {
            localStorage.removeItem(this.FACT_HISTORY_KEY);
        }

        public getFactIdFromQueryString(): number {
            let search = new URLSearchParams(this.$window.location.search);

            return parseInt(search.get('fact') || search.get('?fact') || '-1');
        }

        public getFactIds(): FactModel[] {
            if (!Array.prototype.map) {
                return null;
            }

            let factHistory: FactModel[] = (JSON.parse(this._getItem(this.FACT_HISTORY_KEY)) || [])
                .map(function (element: { ID: number, Timestamp: string, ExpirationDate: string}): FactModel {
                    return {
                        ID: element.ID,
                        // Number of seconds since 1/1/1970
                        Timestamp: new Date(element.Timestamp),
                        ExpirationDate: new Date(element.ExpirationDate)
                    };
                });
            // Get an array with the newest fact IDs first
            return factHistory
                .sort((a, b) => a.Timestamp.valueOf() - b.Timestamp.valueOf())
                .reverse();
        }

        public async getFactIdsRemovingExpiredAsync(serverTimeInfo?: ServerTimeInfo): Promise<FactModel[]> {
            if (typeof serverTimeInfo === 'undefined') {
                serverTimeInfo = (await this._getServerTimeInfoAsync()).data;
            }

            let factHistory: FactModel[] = this.getFactIds();
            let currentDate = serverTimeInfo.now.valueOf();

            return factHistory
                .filter(f => f.ExpirationDate.valueOf() > currentDate)
                .sort((a, b) => a.Timestamp.valueOf() - b.Timestamp.valueOf());
        }
    }

    // TypeScript compiles classes down to a constructor function.
    lightstreamAppModule.service('factHistory', ['$http', '$window', FactHistory]);

    lightstreamAppModule.run(['factHistory', function (factHistory: FactHistory) {
        let fact = factHistory.getFactIdFromQueryString();

        if (fact !== -1) factHistory.addFactIdAsync(fact);
    }]);
}