/// <binding AfterBuild='default' />
var config = require("./terrific.json");
var gulp = require("gulp");
var concatCss = require("gulp-concat-css");
var less = require("gulp-less");

gulp.task("default", function () {
   return gulp.src(config["app.css"])
      .pipe(less())
      .pipe(concatCss("app.css"))
      .pipe(gulp.dest("wwwroot/"));
});