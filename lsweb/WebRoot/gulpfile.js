"use strict";
exports.__esModule = true;
var gulp = require("gulp");
var del = require("del");
var gConcat = require("gulp-concat");
var changed = require("gulp-changed");
var gzip = require("gulp-gzip");
var newer = require("gulp-newer");
var sourcemaps = require("gulp-sourcemaps");
var uglify = require("gulp-uglify");
var components = require("./gulp-ng");
var scriptsAdobeTargetBundle = [
    './Scripts/VisitorAPI_Suntrust.js',
    './Scripts/at.js'
];
var badWordsBundle = [
    './node_modules/bad-words/lib/lang.json'
];
var scriptsAdobeTargetBodyHidingBundle = [
    './Scripts/VisitorAPI_Suntrust.js',
    './Scripts/at-bodyhiding.js'
];
var scriptsMarketingBundle = [
    './Scripts/app/SearchTerms.js',
    './Scripts/app/jquery.cookie.js',
    './Scripts/app/code_to_paste.js',
    './Scripts/app/marketing.js'
];
function performBundle(input, output, isDebug) {
    if (isDebug) {
        return gulp.src(input)
            .pipe(newer('./bundles/' + output))
            .pipe(sourcemaps.init())
            .pipe(gConcat(output))
            .pipe(sourcemaps.write())
            .pipe(gulp.dest('./bundles/'));
    }
    else {
        return gulp.src(input)
            .pipe(newer('./bundles/' + output))
            .pipe(gConcat(output))
            .pipe(uglify())
            .pipe(gulp.dest('./bundles/'));
    }
}
function performGZip(input) {
    return input
        .pipe(changed('./', {
        transformPath: function (p) { return p + '.gz'; }
    }))
        .pipe(gzip({
        append: true,
        gzipOptions: {
            level: 9
        }
    }))
        .pipe(gulp.dest('./'));
}
function cleanBundles() {
    return del('./bundles/**/*');
}
function cleanGzip() {
    return del('./**/*.gz');
}
function compressCssGzip() {
    return performGZip(gulp.src('./**/*.css'));
}
function compressJSGzip() {
    return performGZip(gulp.src('./**/*.js'));
}
function compressSvgGzip() {
    return performGZip(gulp.src('./**/*.svg'));
}
function concatJSAdobeTarget() {
    return performBundle(scriptsAdobeTargetBundle, "scripts-adobe-target.js", true);
}
function concatJSAdobeTargetBodyHiding() {
    return performBundle(scriptsAdobeTargetBodyHidingBundle, "scripts-adobe-target-bodyhiding.js", true);
}
function concatJSMarketingBundle() {
    return performBundle(scriptsMarketingBundle, "scripts-marketing.js", true);
}
var concatJS = gulp.parallel(concatJSAdobeTarget, concatJSAdobeTargetBodyHiding, concatJSMarketingBundle);
function minifyJSAdobeTarget() {
    return performBundle(scriptsAdobeTargetBundle, "scripts-adobe-target.js");
}
function minifyJSAdobeTargetBodyHiding() {
    return performBundle(scriptsAdobeTargetBodyHidingBundle, "scripts-adobe-target-bodyhiding.js");
}
function minifyJSMarketingBundle() {
    return performBundle(scriptsMarketingBundle, "scripts-marketing.js");
}
function minifyBadWordsList() {
    return performBundle(badWordsBundle, "bad-words.json", true);
}
var minifyJS = gulp.parallel(minifyJSAdobeTarget, minifyJSAdobeTargetBodyHiding, minifyJSMarketingBundle, minifyBadWordsList);
exports.clean = gulp.parallel(components.cleanDist, cleanBundles, cleanGzip);
exports.compress = gulp.parallel(compressCssGzip, compressJSGzip, compressSvgGzip);
exports.concat = gulp.parallel(concatJS);
exports.minify = gulp.series(minifyJS);
exports.debug = gulp.series(exports.concat, components.debug);
exports.release = gulp.series(exports.minify, exports.compress, components.release);
//# sourceMappingURL=gulpfile.js.map