const path = require('path');
const fs = require('fs');
const gulp = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const sourcemaps = require('gulp-sourcemaps');
const cleanCss = require('gulp-clean-css');
const rename = require('gulp-rename');
const lazypipe = require('lazypipe');
const postcss = require('gulp-postcss');
const autoprefixer = require('autoprefixer');
const clean = require('gulp-clean');

const bootstrapThemeDirs = [
    "bootstrap-external",
    "purple",
    "office-white",
    "blazing-berry",
    "blazing-dark"
];

const getThemeNamesByDir = themeDirName => {
    return [
        `${themeDirName}.bs4`,
        `${themeDirName}.bs5`
	];
}

const bootstrapThemes = [];
bootstrapThemeDirs.forEach(themeDirName => {
    getThemeNamesByDir(themeDirName).forEach(theme => bootstrapThemes.push(theme));
})

const srcSCSS = "scss";
const distCSS = () => {
    return path.join(process.cwd(), "wwwroot");
}

const compileTheme = (theme) => {
    return lazypipe()
        .pipe(sourcemaps.init)
        .pipe(() => sass().on('error', sass.logError))
        .pipe(postcss, [autoprefixer({
            browsers: [
                'Chrome >= 35',
                'Firefox >= 38',
                'Edge >= 12',
                'Explorer >= 10',
                'iOS >= 8',
                'Safari >= 8',
                'Android 2.3',
                'Android >= 4',
                'Opera >= 12']
        })
        ])
        .pipe(sourcemaps.write)
        .pipe(rename, { basename: theme });
}

const minifyCss = lazypipe()
    .pipe(cleanCss)
    .pipe(rename, { suffix: '.min' });

bootstrapThemes.forEach(theme => {
    gulp.task('compile-' + theme, () => {
        const dirName = theme.split('.')[0];
        return gulp.src(srcSCSS + '/' + dirName + `/${theme}.scss`)
            .pipe(compileTheme(theme)())
            .pipe(gulp.dest(distCSS()))
            .pipe(minifyCss())
            .pipe(gulp.dest(distCSS()));
    });
});

gulp.task('watch-themes', cb => {
    const coreDirs = fs.readdirSync(srcSCSS)
        .filter(f => bootstrapThemeDirs.indexOf(f) < 0)
        .map(f => `${srcSCSS}/${f}/**/*.scss`);
    gulp.watch(coreDirs, gulp.series('build-all-themes'));

    bootstrapThemeDirs.forEach(themeDirName => {
        const tasks = getThemeNamesByDir(themeDirName).map(theme => "compile-" + theme);
        gulp.watch(`${srcSCSS}/${themeDirName}/**/*.scss`, gulp.parallel(tasks));
        cb();
    });

    cb();
});

gulp.task('clean-dir', () =>
    gulp.src(distCSS(), {
        read: false,
        allowEmpty: true
    })
    .pipe(clean())
);
gulp.task('build-all-themes', gulp.parallel(bootstrapThemes.map(theme => "compile-" + theme)));
gulp.task('default', gulp.series('clean-dir', 'build-all-themes'));
gulp.task('watch', gulp.series('default', 'watch-themes'));

