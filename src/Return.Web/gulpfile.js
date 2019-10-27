/// <binding ProjectOpened='sass:watch' />
'use strict';

const gulp = require('gulp');
const sass = require('gulp-sass');
const autoprefixer = require('gulp-autoprefixer');

sass.compiler = require('node-sass');

gulp.task('sass', function() {
    return gulp
        .src('./_scss/main.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(autoprefixer({ cascade: false }))
        .pipe(gulp.dest('./wwwroot/build/css/'));
});

gulp.task('copy-fonts', function() {
    return gulp
        .src('./node_modules/@fortawesome/fontawesome-free/webfonts/*')
        .pipe(gulp.dest('./wwwroot/build/fonts/'));
});

gulp.task('copy-js-deps', function() {
    return gulp
        .src('./node_modules/blazor.polyfill/blazor.polyfill.min.js')
        .pipe(gulp.dest('./wwwroot/build/js/compat'));
});

gulp.task('sass:watch', function() {
    gulp.watch('./_scss/**/*.scss', gulp.series('sass'));
});

gulp.task('clean', function(cb) {
    del(['./wwwroot/build'], cb);
});

gulp.task('build', gulp.parallel('sass', 'copy-fonts', 'copy-js-deps'));

gulp.task('default', gulp.series('clean', 'build'));
