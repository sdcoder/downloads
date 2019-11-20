interface HighlightOptions {
    wordsOnly: boolean;
    caseSensitive: boolean;
    element: string;
    className: string;
}

interface JQuery {
    highlight(node: string | string[], options?: HighlightOptions): JQuery;
    unhighlight(options?: HighlightOptions): JQuery;
}