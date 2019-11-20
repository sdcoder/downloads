var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var LightStreamWeb;
(function (LightStreamWeb) {
    var Frontend;
    (function (Frontend) {
        var AjaxModalController = /** @class */ (function () {
            function AjaxModalController(modalSelector) {
                this._$modal = $(modalSelector);
            }
            AjaxModalController.prototype.setUrl = function (url) {
                var iframe = this._$modal.children("iframe")[0];
                var $iframe = $(iframe);
                $iframe.prop("src", url);
                $iframe.on('load', function () {
                    var doc = iframe.contentDocument || iframe.contentWindow.document;
                    var html = doc.documentElement;
                    var maxHeight = Math.max(html.offsetHeight, doc.body.scrollHeight);
                    $(this).height(maxHeight + "px");
                    $iframe.unbind('load');
                });
            };
            AjaxModalController.prototype.open = function () {
                this._$modal.foundation('open');
            };
            return AjaxModalController;
        }());
        Frontend.AjaxModalController = AjaxModalController;
        var PrintableAjaxModalController = /** @class */ (function (_super) {
            __extends(PrintableAjaxModalController, _super);
            function PrintableAjaxModalController(modalSelector, printButtonSelector) {
                var _this = _super.call(this, modalSelector) || this;
                _this._$printButton = $(printButtonSelector);
                _this._$printButton.click(function () {
                    var iframe = _this._$modal.children("iframe")[0];
                    var contentWindow = iframe.contentWindow;
                    var result = contentWindow.document.execCommand('print', false, null);
                    if (!result) {
                        contentWindow.print();
                    }
                });
                return _this;
            }
            return PrintableAjaxModalController;
        }(AjaxModalController));
        Frontend.PrintableAjaxModalController = PrintableAjaxModalController;
    })(Frontend = LightStreamWeb.Frontend || (LightStreamWeb.Frontend = {}));
})(LightStreamWeb || (LightStreamWeb = {}));
//# sourceMappingURL=ModalControllers.js.map