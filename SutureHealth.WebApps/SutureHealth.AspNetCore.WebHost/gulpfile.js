/// <binding BeforeBuild='default' Clean='cleanup' ProjectOpened='watch' />
var path = require('path'),
    gulp = require('gulp'),
    rename = require('gulp-rename'),
    gp_clean = require('gulp-clean'),
    cleancss = require('gulp-clean-css'),
    sassy = require('gulp-sass')(require('sass')),
    ts = require("gulp-typescript"),
    tsProject = ts.createProject("tsconfig.json");

var scssPath = path.resolve(__dirname, "Styles");
var webPath = path.resolve(__dirname, "wwwroot");

var srcPaths = {
    sass: [
        path.resolve(scssPath, '*.scss')
    ],
    css: [
        path.resolve(webPath, "css/Email.min.css")
    ],
    js: [
        "./node_modules/twilio-video/dist/twilio-video.min.js",
        "./node_modules/require.js/require.min.js"
    ],
    ts: [
        "./scripts/**.ts"
    ]
};

var destPaths = {
    css: path.resolve(webPath, 'css'),
    js: path.resolve(webPath, 'js')
};

/* SASS/CSS */
gulp.task('sass_clean', function (done) {
    done();
    return; // Short-circuit and don't remove files from wwwroot/css
    gulp.src(destPaths.css + "*.*", { read: false })
        .pipe(gp_clean({ force: true }));
});

gulp.task('sass', function (done) {
    gulp.src(srcPaths.sass)
        .pipe(sassy({ outputStyle: 'compressed' }))
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest(destPaths.css));
    done();
});

gulp.task('watch-sass', function () {
    gulp.watch("./Styles/**/*.scss", gulp.series(['sass', 'emails']));
})

/*JS*/
gulp.task('js', function (done) {
    gulp.src(srcPaths.js)
        .pipe(gulp.dest(destPaths.js));

    done();
})

gulp.task('ts', function (done) {
    tsProject.src().pipe(tsProject()).js.pipe(gulp.dest(destPaths.js));
    done();
})

gulp.task('watch-ts', function () {
    gulp.watch(tsProject.options.rootDirs, gulp.series(['ts']));
})

/*CSS*/
gulp.task('emails', function (done) {  
    const postcss = require('gulp-postcss');
    const sourcemaps = require('gulp-sourcemaps');

    gulp.src(path.resolve(scssPath, 'Email.scss'))
        .pipe(sassy())
        .pipe(postcss([require('autoprefixer'), require('postcss-nested'), require('oldie')]))
        .pipe(cleancss({ compatibility: 'ie8' }))
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest(destPaths.css))
    done();
})

/* Defaults */
gulp.task('cleanup', gulp.series(['sass_clean']));
gulp.task('default', gulp.parallel(['sass', 'emails', 'js']));
gulp.task('watch', gulp.parallel(['watch-sass', 'watch-ts']));