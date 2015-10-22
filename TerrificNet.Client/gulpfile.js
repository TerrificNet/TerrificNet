/// <binding AfterBuild='compile' Clean='clean' ProjectOpened='restore' />
var gulp = require('gulp');
var bower = require('gulp-bower');
var tsd = require('gulp-tsd');
var tsc = require('gulp-typescript');
var sourcemaps = require('gulp-sourcemaps');
var jasmineBrowser = require('gulp-jasmine-browser');
var browserify = require('browserify');
var source = require('vinyl-source-stream');
var del = require('del');

var srcProject = tsc.createProject('./tsconfig.json');
var testProject = tsc.createProject('./tsconfig.json');

var config = {
    out: "./built/local",
    deploy: "./wwwroot"
};

gulp.task('bower', function () {
    return bower()
        .pipe(gulp.dest('bower_components'));
});

gulp.task('tsd', function (callback) {
    return tsd({
        command: 'reinstall',
        config: './tsd.json'
    }, callback);
});

/**
 * Compile TypeScript and include references to library and app .d.ts files.
 */
var compile = function(cConfig) {
    var sourceTsFiles = [cConfig.src];

    var tsResult = gulp.src(sourceTsFiles)
                       .pipe(sourcemaps.init())
                       .pipe(tsc(config.project));

    tsResult.dts.pipe(gulp.dest(cConfig.out));
    return tsResult.js
                    .pipe(sourcemaps.write('.'))
                    .pipe(gulp.dest(cConfig.out));
};

gulp.task('compile:src', function () {
    return compile({ src: "./src/**/*.ts", out: config.out + "/src", project: config.srcProject });
});

gulp.task('compile:test', ['compile:src'], function () {
    return compile({ src: "./test/**/*.ts", out: config.out + "/test", project: config.testProject });
});

gulp.task('clean:src', function() {
    return del([config.out + "/src"]);
});

gulp.task('clean:test', function () {
    return del([config.out + "/test"]);
});

gulp.task('compile', ['compile:src', 'compile:test']);
gulp.task('clean', ['clean:src', 'clean:test']);
gulp.task('restore', ['tsd', 'bower']);

gulp.task('bundle_js', ['compile-ts'], function() {
    return gulp.src(config.out + "/src/**/*.js")
        .pipe(sourcemaps.init({ loadMaps: true }))
        // TODO
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('dist'));
});

gulp.task('test', ['compile'], function () {

    return gulp.src([
            './bower_components/**/*.min.js',
            config.out + '/**/*.js'
        ])
        .pipe(jasmineBrowser.specRunner({console: true}))
        .pipe(jasmineBrowser.headless());
});
