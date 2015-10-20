/// <binding AfterBuild='compile-ts' />
var gulp = require('gulp');
var bower = require('gulp-bower');
var tsd = require('gulp-tsd');
var tsc = require('gulp-typescript');
var sourcemaps = require('gulp-sourcemaps');
var jasmineBrowser = require('gulp-jasmine-browser');
var browserify = require('browserify');
var source = require('vinyl-source-stream');

var tsProject = tsc.createProject('./tsconfig.json');
var config = {
    tsOutputPath: "./built/local"
};

gulp.task('bower', function () {
    return bower()
        .pipe(gulp.dest('wwwroot/lib/'));
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
gulp.task('compile-ts', function () {
    var sourceTsFiles = ["./src/**/*.ts",
                        "./test/**/*.ts",
                         "./typings/**/*.ts"]; //reference to library .d.ts files

    var tsResult = gulp.src(sourceTsFiles)
                       .pipe(sourcemaps.init())
                       .pipe(tsc(tsProject));

    tsResult.dts.pipe(gulp.dest(config.tsOutputPath));
    return tsResult.js
                    .pipe(sourcemaps.write('.'))
                    .pipe(gulp.dest(config.tsOutputPath));
});

gulp.task('bundle_js', ['compile-ts'], function() {
    gulp.src(config.tsOutputPath + "/.*js")
        .pipe(sourcemaps.init({ loadMaps: true }))
        // TODO
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('dist'));
});

gulp.task('test', ['compile-ts'], function () {

    //var browserified = transform(function (filename) {
    //    var b = browserify(filename);
    //    return b.bundle();
    //});

    //return gulp.src(['./built/local/*.js'])
    //  .pipe(browserified)
    //  .pipe(gulp.dest('./dist'));

    return browserify('./built/local/mydummytest.js')
        .bundle()
        //Pass desired output filename to vinyl-source-stream
        .pipe(source('bundle.js'))
        // Start piping stream to tasks!
        .pipe(gulp.dest('./built/'));

    //return gulp.src([
    //        './wwwroot/lib/**/*.min.js',
    //        config.tsOutputPath + '/mydummytest.js',
    //        config.tsOutputPath + '/mydummy.js'
    //    ])
    //    .pipe(jasmineBrowser.specRunner({console: true}))
    //    .pipe(jasmineBrowser.headless());
});
