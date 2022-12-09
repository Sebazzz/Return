/// <binding ProjectOpened='sass:watch' />
'use strict';

const gulp = require('gulp');
const dartSass = require('gulp-dart-sass');
const autoprefixer = require('gulp-autoprefixer');

const devBuild = (process.env.NODE_ENV || 'development').trim().toLowerCase() === 'development';
const sassOptions = {
    outputStyle: devBuild ? 'expanded' : 'compressed',
    sourceMapEmbed: devBuild,
    sourceMapContents: devBuild,
    sourceMap: devBuild,
};
dartSass.compiler = require('sass');

gulp.task('sass', function () {
    return gulp
        .src('./_scss/main.scss')
        .pipe(dartSass(sassOptions).on('error', dartSass.logError))
        .pipe(autoprefixer({ cascade: false }))
        .pipe(gulp.dest('./wwwroot/build/css/'));
});

gulp.task('copy-fonts', function () {
    return gulp
        .src('./node_modules/@fortawesome/fontawesome-free/webfonts/*')
        .pipe(gulp.dest('./wwwroot/build/fonts/'));
});

gulp.task('sass:watch', function () {
    gulp.watch('./_scss/**/*.scss', gulp.series('sass'));
});

gulp.task('clean', function (cb) {
    del(['./wwwroot/build'], cb);
});

gulp.task('build', gulp.parallel('sass', 'copy-fonts'));

gulp.task('default', gulp.series('clean', 'build'));
