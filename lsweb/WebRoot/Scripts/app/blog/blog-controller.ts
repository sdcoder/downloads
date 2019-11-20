namespace LightStreamWeb.Frontend {
    interface Post {
        title: string;
        summary: string;
        imageUrl: string;
        blogPostUrl: string;
    }

    class BlogController {
        public isHidden = true;
        public posts: Post[];

        constructor(private _$scope: any, private _$sce: angular.ISCEService, private jQuery: JQuery, private _blogService: LightStreamWeb.Frontend.BlogService) {
            this._$scope.onImageLoaded = (event) => this._resizeBlogSummaryHeight($(event.currentTarget).closest('.widget-blog.row'));
        }
        
        private _resizeBlogSummaryHeight(blogRow) {

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
            else if ($(blogPost).text().slice(-1) === '.') $(blogPost).text($(blogPost).text() + '..');
            else $(blogPost).text($(blogPost).text() + '...');
        }

        public init(options) {
                this._blogService.getPosts({
                    rssUrl: options.config.rssUrl,
                    limit: options.config.limit
                }).then(response => {
                    this.posts = response.data.posts.map(p => {
                        if (this.isHidden)
                            this.isHidden = false;

                        return {
                            title: p.Title,
                            summary: this._$sce.trustAsHtml(p.Summary.trim()),
                            imageUrl: p.ImageUrl,
                            blogPostUrl: p.BlogPostUrl
                        };
                    });

                    $(window).on('resize', e => {
                        if ($('.widget-blog').length == 0)
                            return;

                        $.each($('.widget-blog.row'), (idx, elem) => {
                            this._resizeBlogSummaryHeight(elem);
                        });
                    });

                }, e => {
                    console.log(e);
                });
        }
    }

    angular.module('LightStreamApp').controller('BlogController', ['$scope', '$sce', 'jQuery', 'BlogService', BlogController]);
}
