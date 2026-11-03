document.addEventListener("DOMContentLoaded", function () {
    const editorElement = document.getElementById("note-editor");
    const contentInput = document.getElementById("note-content");

    if (!editorElement || !contentInput) {
        return;
    }

    const quill = new Quill("#note-editor", {
        theme: "snow",
        modules: {
            toolbar: [
                [{ header: [1, 2, 3, false] }],
                ["bold", "italic", "underline", "strike"],
                [{ list: "ordered" }, { list: "bullet" }],
                ["blockquote", "code-block"],
                ["link"],
                ["clean"]
            ]
        }
    });

    if (contentInput.value) {
        quill.clipboard.dangerouslyPasteHTML(contentInput.value);
    }

    quill.on("text-change", function () {
        contentInput.value = quill.getText().trim()
            ? quill.getSemanticHTML()
            : "";
    });
});