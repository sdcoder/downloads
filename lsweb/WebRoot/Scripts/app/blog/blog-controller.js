var LightStreamWeb;
(function (LightStreamWeb) {
    var Frontend;
    (function (Frontend) {
        var BlogController = /** @class */ (function () {
            function BlogController(_$scope, _$sce, jQuery, _blogService) {
                var _this = this;
                this._$scope = _$scope;
                this._$sce = _$sce;
                this.jQuery = jQuery;
                this._blogService = _blogService;
                this.isHidden = true;
                this._$scope.onImageLoaded = function (event) { return _this._resizeBlogSummaryHeight($(event.currentTarget).closest('.widget-blog.row')); };
            }
            BlogController.prototype._resizeBlogSummaryHeight = function (blogRow) {
                //-- get new blog post summary height
                var imageHeight = $('.blogpost-image img', blogRow).height();
                var titleHeight = $('.blogpost-title', blogRow).height();
                var readMoreheight = $('.blogpost-readmore', blogRow).height();
                var blogPost = $('.blogpost-summary', blogRow).get(0);
                var height = imageHeight - titleHeight - readMoreheight;
                //-- reset blog summary text
                $(blogPost).html($(blogPost).data('blog-origtext'));
                //-- set blog summary post height to same height as image
                $('.blogpost-summary', blogRow).css({
                    'height': 'auto',
                    'min-height': 'auto',
                    'text-overflow': 'ellipsis'
                });
                //-- on mobile devices show full text
                if (window.innerWidth <= 726) {
                    $('.blogpost-summary', blogRow).css({
                        'height': 'auto',
                        'min-height': 'auto'
                    });
                    return;
                }
                //-- truncate blog summary text
                var parts = $(blogPost).text().split(' ');
                //remove the last word of the text until it fits the specified height
                while (parts.pop() && $(blogPost).height() > height) {
                    $(blogPost).text(parts.join(' '));
                }
                if ($(blogPost).text().slice(-3) === '...') { }
                else if ($(blogPost).text().slice(-1) === '.')
                    $(blogPost).text($(blogPost).text() + '..');
                else
                    $(blogPost).text($(blogPost).text() + '...');
            };
            BlogController.prototype.init = function (options) {
                var _this = this;
                this._blogService.getPosts({
                    rssUrl: options.config.rssUrl,
                    limit: options.config.limit
                }).then(function (response) {
                    _this.posts = response.data.posts.map(function (p) {
                        if (_this.isHidden)
                            _this.isHidden = false;
                        return {
                            title: p.Title,
                            summary: _this._$sce.trustAsHtml(p.Summary.trim()),
                            imageUrl: p.ImageUrl,
                            blogPostUrl: p.BlogPostUrl
                        };
                    });
                    $(window).on('resize', function (e) {
                        if ($('.widget-blog').length == 0)
                            return;
                        $.each($('.widget-blog.row'), function (idx, elem) {
                            _this._resizeBlogSummaryHeight(elem);
                        });
                    });
                }, function (e) {
                    console.log(e);
                });
            };
            return BlogController;
        }());
        angular.module('LightStreamApp').controller('BlogController', ['$scope', '$sce', 'jQuery', 'BlogService', BlogController]);
    })(Frontend = LightStreamWeb.Frontend || (LightStreamWeb.Frontend = {}));
})(LightStreamWeb || (LightStreamWeb = {}));
//# sourceMappingURL=blog-controller.js.map