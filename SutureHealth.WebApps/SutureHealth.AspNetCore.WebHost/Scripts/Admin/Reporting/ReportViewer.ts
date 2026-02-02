(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const reportFrame = document.querySelector(".report-content[role='main']") as HTMLElement;
        if (reportFrame) {
            let shadow = reportFrame.attachShadow({ mode: 'open' });
            if (shadow && reportFrame.dataset.content) {
                const reports = document.querySelectorAll(reportFrame.dataset.content);
                if (reports.length > 0) {
                    const report = reports[0];
                    shadow.appendChild(report.cloneNode(true));
                }
            }
        }
    }, false);
})();