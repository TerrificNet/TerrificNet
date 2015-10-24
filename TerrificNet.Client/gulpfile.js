/// <binding AfterBuild='compile' Clean='clean' ProjectOpened='restore' />
var gulp = require('gulp');
var bower = require('gulp-bower');
var tsd = require('gulp-tsd');
var tsc = require('gulp-typescript');
var sourcemaps = require('gulp-sourcemaps');
var jasmineBrowser = require('gulp-jasmine-browser');
var browserify = require('browserify');
var source = require('vinyl-source-stream');
var transform = require('vinyl-transform');
var uglify = require('gulp-uglify');
var dnx = require('gulp-dnx');
var del = require('del');

var srcProject = tsc.createProject({
    target: "es5",
    module: "commonjs"
});
var testProject = tsc.createProject({
    target: "es5",
    module: "commonjs"
});

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
                       .pipe(tsc(cConfig.project));

    tsResult.dts.pipe(gulp.dest(cConfig.out));
    return tsResult.js
                    .pipe(sourcemaps.write('.'))
                    .pipe(gulp.dest(cConfig.out));
};

gulp.task('compile:src', function () {
    return compile({ src: "./src/**/*.ts", out: config.out + "/src", project: srcProject });
});

gulp.task('compile:test', ['compile:src'], function () {
    return compile({ src: "./test/**/*.ts", out: config.out + "/test", project: testProject });
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


gulp.task('bundle_js', ['compile'], function () {
    var props = { entries: [config.out + "/test/integrationtest.js"], debug: true };
    var bundler = browserify(props);
    var stream = bundler.bundle();
    return stream
        .pipe(source(config.out + "/test/bundle.js"))
        .pipe(gulp.dest("."));

    //var browserified = transform(function (filename) {
    //    var b = browserify(filename);
    //    return b.bundle();
    //});

    //return gulp.src([config.out + "/src/**/*.js"])
    //    .pipe(browserified)
    //    //.pipe(uglify())
    //    //.pipe(sourcemaps.init({ loadMaps: true }))
    //    //.pipe()
    //    //.pipe(sourcemaps.write('.'))
    //    .pipe(gulp.dest('./dist'));
});

gulp.task('test', ['bundle_js'], function () {

    //dnx('kestrel');

    return gulp.src([
            config.out + '/test/bundle.js'
        ])
        .pipe(jasmineBrowser.specRunner(/*{ console: true }*/))
        .pipe(jasmineBrowser.server({ port: 8888 }));
    //.pipe(jasmineBrowser.headless());
});
