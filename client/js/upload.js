//preventing page from redirecting when file drops
$("html").on("dragover", function (e) {
    e.preventDefault();
    e.stopPropagation();
    $('#fw-upload').addClass('drop');
});
$("html").on("drop", function (e) {
    e.preventDefault();
    e.stopPropagation();
});

let fwUpload = $('#fw-upload');

let startUpload = function (files) {
    console.log(files)
}

$("#fw-upload").on("drop", function (e) {
    this.className = 'upload-drop-zone';
    startUpload(e.dataTransfer.files)
});

fwUpload.ondragleave = function () {
    this.className = 'upload-drop-zone';
    return false;
}

