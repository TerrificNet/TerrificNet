/// <binding />
var gulp = require('gulp');
var tsc = require('gulp-typescript');
var sourcemaps = require('gulp-sourcemaps');
var jasmineBrowser = require('gulp-jasmine-browser');
var uglify = require('gulp-uglify');
var del = require('del');
var tsify = require('tsify');
var gulpTypings = require('gulp-typings');
var gulp = require('gulp');
var webpack = require('webpack-stream');
var watch = require('gulp-watch');
var JasminePlugin = require('gulp-jasmine-browser/webpack/jasmine-plugin');

gulp.task('default', function () {
   return gulp.src('src/*.ts')
     .pipe(webpack({
        output: {
           filename: 'index.js'
        },
        devtool: 'source-map',
        resolve: {
           modulesDirectories: ['node_modules', 'src'],
           extensions: ['', '.webpack.js', '.web.js', '.ts', '.js']
        },
        module: {
           loaders: [
               { test: /\.ts$/, loader: 'ts-loader' }
           ]
        }
     }))
     .pipe(gulp.dest('dist/npm/'));
});

gulp.task("install_typings", function () {
   var stream = gulp.src("./typings.json")
       .pipe(gulpTypings());

   return stream;
});

gulp.task("build-npm", ["default"], function() {
   

});

gulp.task('test', function () {

   var plugin = new JasminePlugin();

   return gulp.src('test/integrationtest.ts')
   .pipe(webpack({
      output: {
         filename: 'main.js'
      },
      watch: true,
      devtool: 'source-map',
      resolve: {
         modulesDirectories: ['node_modules', 'src'],
         extensions: ['', '.webpack.js', '.web.js', '.ts', '.js']
      },
      module: {
         loaders: [
             { test: /\.ts$/, loader: 'ts-loader' }
         ]
      },
      plugins: [plugin]
   }))
   .pipe(jasmineBrowser.specRunner())
   .pipe(jasmineBrowser.server({ port: 8888, whenReady: plugin.whenReady }));
});
