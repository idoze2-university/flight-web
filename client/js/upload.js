//preventing page from redirecting when file drops
$("html").on("dragover", function (e) {
    e.preventDefault();
    e.stopPropagation();
    $('#fw-upload').addClass('drop');
    $('#fw-upload').innerHTML = "Drop the file anywhere to upload.";
});
$("html").on("drop", function (e) {
    e.preventDefault();
    e.stopPropagation();
    $('#fw-upload').removeClass('drop');
    // console.log(e);
    ajax_upload(e.originalEvent.dataTransfer.files);
});
$("html").on("dragleave", function (e) {
    $('#fw-upload').removeClass('drop');
});

async function ajax_upload(files) {
    var fd = new FormData();
    fd.append('file', files);
    await $.ajax({
        url: `${server}/api/FlightPlan`,
        type: 'POST',
        data: fd,
        success: function (response) {
            if (response != 0) {
                alert('file uploaded');
            } else {
                alert('file not uploaded');
            }
        },
    });
}

