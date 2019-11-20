namespace LightStreamWeb.Frontend {
    export class AjaxModalController {
        protected _$modal: JQuery;

        constructor(modalSelector: string) {
            this._$modal = $(modalSelector);
        }

        public setUrl(url: string) {
            let iframe = this._$modal.children("iframe")[0] as HTMLIFrameElement;
            let $iframe = $(iframe);
            $iframe.prop("src", url);

            $iframe.on('load', function(){
                let doc = iframe.contentDocument || iframe.contentWindow.document;
                let html = doc.documentElement;
                var maxHeight = Math.max(html.offsetHeight, doc.body.scrollHeight);

                $(this).height(`${maxHeight}px`);

                $iframe.unbind('load');
            });
        }

        public open() {
            this._$modal.foundation('open');
        }
    }

    export class PrintableAjaxModalController extends AjaxModalController {
        private _$printButton: JQuery;

        constructor(modalSelector: string, printButtonSelector: string) {
            super(modalSelector);
            this._$printButton = $(printButtonSelector);

            this._$printButton.click(() => {
                let iframe = this._$modal.children("iframe")[0] as HTMLIFrameElement;
                let contentWindow = iframe.contentWindow;
                let result = contentWindow.document.execCommand('print', false, null);

                if (!result) {
                    contentWindow.print();
                }
            });
        }
    }
}