import * as gulp from 'gulp';
import * as del from 'del';

import * as gConcat from 'gulp-concat';
import * as changed from 'gulp-changed';
import * as gzip from 'gulp-gzip';
import * as newer from 'gulp-newer';
import * as sourcemaps from 'gulp-sourcemaps';
import * as uglify from 'gulp-uglify';

import * as components from './gulp-ng';

const scriptsAdobeTargetBundle = [
    './Scripts/VisitorAPI_Suntrust.js',
    './Scripts/at.js'
];

const badWordsBundle = [
    './node_modules/bad-words/lib/lang.json'
];

const scriptsAdobeTargetBodyHidingBundle = [
    './Scripts/VisitorAPI_Suntrust.js',
    './Scripts/at-bodyhiding.js'
];

const scriptsMarketingBundle = [
    './Scripts/app/SearchTerms.js',
    './Scripts/app/jquery.cookie.js',
    './Scripts/app/code_to_paste.js',
    './Scripts/app/marketing.js'
];

function performBundle(input, output, isDebug?: boolean) {
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
            transformPath: p => p + '.gz'
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

function compressCssGzip () {
    return performGZip(gulp.src('./**/*.css'));
}

function compressJSGzip() {
    return performGZip(gulp.src('./**/*.js'));
}

function compressSvgGzip () {
    return performGZip(gulp.src('./**/*.svg'));
}

function concatJSAdobeTarget () {
    return performBundle(scriptsAdobeTargetBundle, "scripts-adobe-target.js", true);
}

function concatJSAdobeTargetBodyHiding () {
    return performBundle(scriptsAdobeTargetBodyHidingBundle, "scripts-adobe-target-bodyhiding.js", true);
}

function concatJSMarketingBundle () {
    return performBundle(scriptsMarketingBundle, "scripts-marketing.js", true);
}

const concatJS = gulp.parallel(concatJSAdobeTarget, concatJSAdobeTargetBodyHiding, concatJSMarketingBundle);

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

const minifyJS = gulp.parallel(minifyJSAdobeTarget, minifyJSAdobeTargetBodyHiding, minifyJSMarketingBundle, minifyBadWordsList);

export const clean = gulp.parallel(components.cleanDist, cleanBundles, cleanGzip);

export const compress = gulp.parallel(compressCssGzip, compressJSGzip, compressSvgGzip);

export const concat = gulp.parallel(concatJS);

export const minify = gulp.series(minifyJS);

export const debug = gulp.series(concat, components.debug);

export const release = gulp.series(minify, compress, components.release);
