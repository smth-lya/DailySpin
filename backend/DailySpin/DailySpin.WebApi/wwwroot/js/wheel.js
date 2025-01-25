// wheel.js

document.addEventListener("DOMContentLoaded", async () => {
    const wheel = document.getElementById("wheel");
    const spinButton = document.getElementById("spinButton");
    const resultDisplay = document.getElementById("result");

    // Fetch segments from the API
    const fetchSegments = async () => {
        const response = await fetch("/api/wheel/segments");
        return response.json();
    };

    // Render the wheel with segments
    const renderWheel = async () => {
        const segments = await fetchSegments();
        const wheelRadius = 500;
        const centerX = wheelRadius;
        const centerY = wheelRadius;
        const totalWeight = segments.reduce((acc, seg) => acc + seg.Weight, 0);

        const svgNamespace = "http://www.w3.org/2000/svg";
        let currentAngle = 0;

        segments.forEach(segment => {
            const sliceAngle = (segment.Weight / totalWeight) * 360;
            const endAngle = currentAngle + sliceAngle;

            const x1 = centerX + wheelRadius * Math.cos((Math.PI * currentAngle) / 180);
            const y1 = centerY + wheelRadius * Math.sin((Math.PI * currentAngle) / 180);
            const x2 = centerX + wheelRadius * Math.cos((Math.PI * endAngle) / 180);
            const y2 = centerY + wheelRadius * Math.sin((Math.PI * endAngle) / 180);

            const largeArc = sliceAngle > 180 ? 1 : 0;

            const path = document.createElementNS(svgNamespace, "path");
            const d = `
                M ${centerX},${centerY}
                L ${x1},${y1}
                A ${wheelRadius},${wheelRadius} 0 ${largeArc},1 ${x2},${y2}
                Z
            `;
            path.setAttribute("d", d);
            path.setAttribute("fill", segment.Color);
            wheel.appendChild(path);

            currentAngle = endAngle;
        });
    };

    // Handle spinning
    spinButton.addEventListener("click", async () => {
        spinButton.disabled = true; // Disable button during spin
        const response = await fetch("/api/wheel/spin", { method: "POST" });
        const result = await response.json();

        // Display result
        resultDisplay.textContent = `You won: ${result.Winner}`;
        spinButton.disabled = false; // Re-enable button
    });

    await renderWheel();
});
