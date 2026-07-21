// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    document.addEventListener("submit", function (event) {
        var form = event.target;
        if (!(form instanceof HTMLFormElement)) {
            return;
        }

        var message = form.getAttribute("data-confirm");
        if (message && !window.confirm(message)) {
            event.preventDefault();
        }
    });

    function hideToast(toast) {
        setTimeout(function () {
            toast.classList.add("is-hiding");
            setTimeout(function () {
                toast.remove();
            }, 240);
        }, 2000);
    }

    function showToast(message) {
        var currentToast = document.querySelector("[data-app-toast]");
        if (currentToast) {
            currentToast.remove();
        }

        var toast = document.createElement("div");
        toast.className = "app-toast";
        toast.setAttribute("data-app-toast", "");
        toast.setAttribute("role", "alert");
        toast.setAttribute("aria-live", "assertive");

        var text = document.createElement("span");
        text.textContent = message;
        toast.appendChild(text);
        document.body.appendChild(toast);
        hideToast(toast);
    }

    var appToast = document.querySelector("[data-app-toast]");
    if (appToast) {
        hideToast(appToast);
    }

    var invalidToastShown = false;
    document.addEventListener("invalid", function (event) {
        var field = event.target;
        if (!(field instanceof HTMLInputElement)
            && !(field instanceof HTMLSelectElement)
            && !(field instanceof HTMLTextAreaElement)) {
            return;
        }

        var form = field.form;
        if (form) {
            form.classList.add("was-validated");
        }

        if (invalidToastShown) {
            return;
        }

        invalidToastShown = true;
        var fieldName = field.getAttribute("placeholder")
            || (field.labels && field.labels.length ? field.labels[0].textContent : "")
            || "trường bắt buộc";
        var message = "Vui lòng kiểm tra " + fieldName.trim().toLowerCase() + ".";

        if (field.validity.typeMismatch && field.type === "email") {
            message = "Email chưa đúng định dạng.";
        } else if (field.validity.patternMismatch && field.title) {
            message = field.title + ".";
        } else if ((field.validity.rangeUnderflow || field.validity.rangeOverflow) && field.type === "number") {
            message = "Giá trị số nhập vào không nằm trong khoảng cho phép.";
        }

        showToast(message);
        setTimeout(function () {
            invalidToastShown = false;
        }, 250);
    }, true);

    var paymentMethods = document.querySelectorAll('input[name="paymentMethod"]');
    var transferPanel = document.querySelector("[data-transfer-panel]");
    function toggleTransferPanel() {
        paymentMethods.forEach(function (item) {
            var label = item.closest(".method-option");
            if (label) {
                label.classList.toggle("is-selected", item.checked);
            }
        });

        if (!transferPanel) {
            return;
        }

        var selected = document.querySelector('input[name="paymentMethod"]:checked');
        transferPanel.hidden = !selected || selected.value !== "Transfer";
    }

    paymentMethods.forEach(function (item) {
        item.addEventListener("change", toggleTransferPanel);
    });

    toggleTransferPanel();

    var dictionaryPanel = document.querySelector("[data-dictionary-panel]");
    var dictionaryInput = document.querySelector("[data-dictionary-input]");
    var dictionaryResult = document.querySelector("[data-dictionary-result]");
    var dictionaryToggle = document.querySelector("[data-dictionary-toggle]");
    var dictionaryClose = document.querySelector("[data-dictionary-close]");

    var dictionaryCache = {};
    var dictionaryTimer;

    function renderDictionaryResult(word, meaning) {
        dictionaryResult.textContent = "";
        var label = document.createElement("strong");
        label.textContent = word + ": ";
        dictionaryResult.appendChild(label);
        dictionaryResult.appendChild(document.createTextNode(meaning));
    }

    function translateWord() {
        if (!dictionaryInput || !dictionaryResult) {
            return;
        }

        var keyword = dictionaryInput.value.trim().toLowerCase();
        if (!keyword) {
            dictionaryResult.textContent = "Nhập một từ để tra nhanh.";
            return;
        }

        if (dictionaryCache[keyword]) {
            renderDictionaryResult(keyword, dictionaryCache[keyword]);
            return;
        }

        dictionaryResult.textContent = "Đang tra cứu...";
        fetch("https://api.mymemory.translated.net/get?q=" + encodeURIComponent(keyword) + "&langpair=en|vi")
            .then(function (response) { return response.ok ? response.json() : Promise.reject(); })
            .then(function (data) {
                var translatedText = data && data.responseData && data.responseData.translatedText;
                if (!translatedText) {
                    dictionaryResult.textContent = "Không tìm thấy nghĩa phù hợp.";
                    return;
                }

                dictionaryCache[keyword] = translatedText;
                renderDictionaryResult(keyword, translatedText);
            })
            .catch(function () {
                dictionaryResult.textContent = "Không thể kết nối API từ điển. Vui lòng thử lại sau.";
            });
    }

    if (dictionaryToggle && dictionaryPanel) {
        dictionaryToggle.addEventListener("click", function () {
            dictionaryPanel.hidden = !dictionaryPanel.hidden;
            if (!dictionaryPanel.hidden && dictionaryInput) {
                dictionaryInput.focus();
            }
        });
    }

    if (dictionaryClose && dictionaryPanel) {
        dictionaryClose.addEventListener("click", function () {
            dictionaryPanel.hidden = true;
        });
    }

    if (dictionaryInput) {
        dictionaryInput.addEventListener("input", function () {
            clearTimeout(dictionaryTimer);
            dictionaryTimer = setTimeout(translateWord, 400);
        });
    }
});
