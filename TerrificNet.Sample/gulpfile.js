/// <binding />
var config = require("./terrific.json");
var gulp = require("gulp");
var webpack_stream = require('webpack-stream');
var webpack = require("webpack");

gulp.task("default", function () {
   return gulp.src('Controllers/*.ts')
     .pipe(webpack_stream({
        output: {
           filename: 'index.js'
        },
        devtool: 'source-map',
        resolve: {
           modulesDirectories: ['node_modules'],
           extensions: ['', '.webpack.js', '.web.js', '.ts', '.js']
        },
        plugins: [
           new webpack.optimize.UglifyJsPlugin({
              comments: false
           })
        ],
        module: {
           loaders: [
               { test: /\.ts$/, loader: 'ts-loader' }
           ]
        }
     }))
     .pipe(gulp.dest('wwwroot/'));
});