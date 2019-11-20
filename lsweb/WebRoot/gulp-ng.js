"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
exports.__esModule = true;
var gulp = require("gulp");
var fs = require("fs");
var glob = require("glob");
var concat = require("gulp-concat");
var child_process_1 = require("child_process");
var del_1 = require("del");
var paths = {
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
exports.cleanDist = function () { return del_1["default"]([paths.distSrc, paths.distDest + '/**'], { force: true }); };
exports.copyDist = function () {
    return gulp.src(paths.bundleFiles)
        .pipe(concat(paths.bundleName))
        .pipe(gulp.dest('Scripts/webcomponents/'));
};
function getMostRecentChangedDateAsync() {
    return __awaiter(this, void 0, void 0, function () {
        var promises, dates;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    promises = [];
                    paths.watchedForChangeFiles.forEach(function (elem) {
                        promises.push(new Promise(function (resolve) {
                            var mostRecentChangedDate = new Date(0);
                            // Glob returns a list of files matching a pattern.
                            // i.e. all files matching ../CMS.Angular/src/**
                            glob(elem, function (err, files) {
                                if (err) {
                                    throw err.message;
                                }
                                files.forEach(function (file) {
                                    var stat = fs.statSync(file);
                                    if (stat.mtime > mostRecentChangedDate) {
                                        mostRecentChangedDate = stat.mtime;
                                    }
                                });
                                resolve(mostRecentChangedDate);
                            });
                        }));
                    });
                    return [4 /*yield*/, Promise.all(promises)];
                case 1:
                    dates = (_a.sent()).filter(function (d) { return d.valueOf() !== 0; }).sort(sortDatesAsc);
                    return [2 /*return*/, dates.pop()];
            }
        });
    });
}
function sortDatesAsc(date1, date2) {
    if (date1 > date2)
        return 1;
    if (date1 < date2)
        return -1;
    return 0;
}
function getCompiledDate() {
    if (!fs.existsSync(paths.compiledOutputFile)) {
        return new Date(0);
    }
    return fs.statSync(paths.compiledOutputFile).mtime;
}
function debug() {
    return __awaiter(this, void 0, void 0, function () {
        var mostRecentChange, compiledDate;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, getMostRecentChangedDateAsync()];
                case 1:
                    mostRecentChange = _a.sent();
                    compiledDate = getCompiledDate();
                    if (!(mostRecentChange >= compiledDate)) return [3 /*break*/, 3];
                    return [4 /*yield*/, new Promise(function (resolve) {
                            gulp.series(exports.cleanDist, buildDebug, exports.copyDist)(function (error) {
                                resolve(error);
                            });
                        })];
                case 2:
                    _a.sent();
                    _a.label = 3;
                case 3: return [2 /*return*/];
            }
        });
    });
}
exports.debug = debug;
function buildDebug() {
    var cwd = process.cwd();
    process.chdir('../LightStreamWeb.ComponentsHost');
    return new Promise(function (resolve) {
        // TODO: an issue with non-prod mode causes error with debug distribution.  need to resolve this before removing the prod flag.
        child_process_1.exec('node node_modules/@angular/cli/bin/ng build --prod', { maxBuffer: 1024 * 1024 }, function (err, stdout, stderr) {
            process.chdir(cwd);
            console.log(stdout);
            console.log(stderr);
            resolve(err);
        });
    });
}
function buildRelease() {
    var cwd = process.cwd();
    process.chdir('../LightStreamWeb.ComponentsHost');
    return new Promise(function (resolve) {
        child_process_1.exec('node node_modules/@angular/cli/bin/ng build --prod', { maxBuffer: 1024 * 1024 }, function (err, stdout, stderr) {
            process.chdir(cwd);
            console.log(stdout);
            console.log(stderr);
            resolve(err);
        });
    });
}
exports.release = gulp.series(exports.cleanDist, buildRelease, exports.copyDist);
exports["default"] = debug;
//# sourceMappingURL=gulp-ng.js.map