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
        transferPanel.hidden = !selected || selected.value !== "BankTransfer";
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
    var dictionaryRequestId = 0;
    var localDictionary = {
        hello: "xin chào",
        goodbye: "tạm biệt",
        thanks: "cảm ơn",
        thank: "cảm ơn",
        course: "khóa học",
        class: "lớp học",
        teacher: "giáo viên",
        student: "học viên",
        payment: "thanh toán",
        tuition: "học phí",
        schedule: "lịch học",
        score: "điểm số",
        attendance: "điểm danh",
        lesson: "bài học",
        lecture: "bài giảng",
        homework: "bài tập về nhà",
        exam: "kỳ thi",
        speaking: "kỹ năng nói",
        listening: "kỹ năng nghe",
        reading: "kỹ năng đọc",
        writing: "kỹ năng viết"
    };

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
        keyword = keyword.replace(/^[^a-z]+|[^a-z]+$/g, "");
        if (!keyword) {
            dictionaryResult.textContent = "Nhập một từ để tra nhanh.";
            return;
        }

        if (!/^[a-z][a-z\s-]*$/.test(keyword)) {
            dictionaryResult.textContent = "Từ điển hiện hỗ trợ từ tiếng Anh.";
            return;
        }

        if (dictionaryCache[keyword]) {
            renderDictionaryResult(keyword, dictionaryCache[keyword]);
            return;
        }

        if (localDictionary[keyword]) {
            dictionaryCache[keyword] = localDictionary[keyword];
            renderDictionaryResult(keyword, localDictionary[keyword]);
            return;
        }

        var requestId = ++dictionaryRequestId;
        dictionaryResult.textContent = "Đang tra cứu...";
        fetch("https://api.mymemory.translated.net/get?q=" + encodeURIComponent(keyword) + "&langpair=en|vi")
            .then(function (response) { return response.ok ? response.json() : Promise.reject(); })
            .then(function (data) {
                if (requestId !== dictionaryRequestId) {
                    return;
                }

                var translatedText = data && data.responseData && data.responseData.translatedText;
                if (!translatedText) {
                    dictionaryResult.textContent = "Không tìm thấy nghĩa phù hợp.";
                    return;
                }

                dictionaryCache[keyword] = translatedText;
                renderDictionaryResult(keyword, translatedText);
            })
            .catch(function () {
                if (requestId !== dictionaryRequestId) {
                    return;
                }

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

    document.body.classList.add("app-loaded");

    if (!window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
        var animatedElements = document.querySelectorAll([
            ".hero-panel",
            ".hero-summary-card",
            ".section-title",
            ".course-card",
            ".stat-card",
            ".content-card",
            ".table-card",
            ".form-card",
            ".detail-main",
            ".side-panel",
            ".profile-summary",
            ".payment-item",
            ".summary-card",
            ".record-card",
            ".resource-card",
            ".class-choice-card",
            ".auth-page",
            ".site-footer"
        ].join(","));

        if ("IntersectionObserver" in window) {
            var revealObserver = new IntersectionObserver(function (entries, observer) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    entry.target.classList.add("is-visible");
                    observer.unobserve(entry.target);
                });
            }, { threshold: 0.12, rootMargin: "0px 0px -40px 0px" });

            animatedElements.forEach(function (element, index) {
                element.classList.add("app-reveal");
                element.style.setProperty("--reveal-delay", Math.min(index % 6, 5) * 45 + "ms");
                revealObserver.observe(element);
            });
        } else {
            animatedElements.forEach(function (element) {
                element.classList.add("is-visible");
            });
        }
    }
});
