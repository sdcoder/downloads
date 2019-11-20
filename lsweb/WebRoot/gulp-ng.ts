import * as gulp from 'gulp';
import * as fs from 'fs';
import * as glob from 'glob';
import * as concat from 'gulp-concat';

import { exec } from 'child_process';
import del from 'del';

const paths = {
    compiledOutputFile: '../LightStreamWeb.ComponentsHost/dist/main.js',
    distSrc: '../LightStreamWeb.ComponentsHost/dist/**/*',
    distDest: 'Scripts/webcomponents/**',
    watchedForChangeFiles: [
        '../LightStreamWeb.ComponentsHost/src/**/*',
        '../LightStreamWeb.ComponentsHost/*.config',
        '../LightStreamWeb.ComponentsHost/*.json',
        '../LightStreamWeb.ComponentsHost/*.js',
        '../LightStreamWeb.ComponentsHost/*.ts',
        '../LightStreamWeb.ComponentsHost/*.md'
    ],
    bundleFiles: [
        '../LightStreamWeb.ComponentsHost/dist/runtime.js',
        '../LightStreamWeb.ComponentsHost/dist/polyfills.js',
        '../LightStreamWeb.ComponentsHost/dist/scripts.js',
        '../LightStreamWeb.ComponentsHost/dist/main.js'
    ],
    bundleName: 'components.js'
};

export const cleanDist = () => del([paths.distSrc, paths.distDest + '/**'], { force: true });

export const copyDist = () =>
        gulp.src(paths.bundleFiles)
            .pipe(concat(paths.bundleName))
            .pipe(gulp.dest('Scripts/webcomponents/'));

async function getMostRecentChangedDateAsync() {
    let promises: Promise<Date>[] = [];

    paths.watchedForChangeFiles.forEach(function (elem) {
        promises.push(new Promise<Date>(function (resolve) {
            let mostRecentChangedDate = new Date(0);

            // Glob returns a list of files matching a pattern.
            // i.e. all files matching ../CMS.Angular/src/**
            glob(elem, function (err, files) {
                if (err) {
                    throw err.message;
                }

                files.forEach(function (file) {
                    let stat = fs.statSync(file);

                    if (stat.mtime > mostRecentChangedDate) {
                        mostRecentChangedDate = stat.mtime;
                    }
                });

                resolve(mostRecentChangedDate);
            });
        }));
    });

    // Filter out instances where no file was found.
    let dates = (await Promise.all(promises)).filter(d => d.valueOf() !== 0).sort(sortDatesAsc);

    return dates.pop();
}

function sortDatesAsc(date1, date2) {
    if (date1 > date2) return 1;
    if (date1 < date2) return -1;
    return 0;
}

function getCompiledDate() {
    if (!fs.existsSync(paths.compiledOutputFile)) {
        return new Date(0);
    }

    return fs.statSync(paths.compiledOutputFile).mtime;
}

export async function debug() {
    let mostRecentChange = await getMostRecentChangedDateAsync();
    let compiledDate = getCompiledDate();

    if (mostRecentChange >= compiledDate) {
        await new Promise(function (resolve) {
            gulp.series(cleanDist, buildDebug, copyDist)(function (error) {
                resolve(error);
            });
        });
    }
}

function buildDebug() {
    let cwd = process.cwd();
    process.chdir('../LightStreamWeb.ComponentsHost');

    return new Promise(function (resolve) {
        // TODO: an issue with non-prod mode causes error with debug distribution.  need to resolve this before removing the prod flag.
        exec('node node_modules/@angular/cli/bin/ng build --prod', { maxBuffer: 1024 * 1024 }, function (err, stdout, stderr) {
            process.chdir(cwd);
            console.log(stdout);
            console.log(stderr);

            resolve(err);
        });
    });
}

function buildRelease() {
    let cwd = process.cwd();
    process.chdir('../LightStreamWeb.ComponentsHost');

    return new Promise(function (resolve) {
        exec('node node_modules/@angular/cli/bin/ng build --prod', { maxBuffer: 1024 * 1024 }, function (err, stdout, stderr) {
            process.chdir(cwd);
            console.log(stdout);
            console.log(stderr);

            resolve(err);
        });
    });
}

export const release = gulp.series(cleanDist, buildRelease, copyDist);
export default debug;
