// script.js

const wheel = document.getElementById("wheel");
const spinButton = document.getElementById("spinButton");
const spinSound = document.getElementById("spinSound");

// Segment data with weights
const segments = [
  { label: "Jackpot 🎉", color: "#ff7e5f", weight: 1 },
  { label: "Try Again 😢", color: "#6a11cb", weight: 3 },
  { label: "50 Coins 💰", color: "#43cea2", weight: 5 },
  { label: "100 Coins 🪙", color: "#f7971e", weight: 2 },
  { label: "Mystery Box 🎁", color: "#fc4a1a", weight: 1 },
  { label: "Extra Spin 🔄", color: "#4b79a1", weight: 2 },
];

let currentRotation = 0;
let isSpinning = false;

// Generate SVG wheel with weighted segments
// script.js

function generateWheel() {
  const totalWeight = segments.reduce((sum, segment) => sum + segment.weight, 0);
  let currentAngle = 0;

  segments.forEach((segment) => {
    const sliceAngle = (segment.weight / totalWeight) * 360;
    const startAngle = currentAngle;
    const endAngle = currentAngle + sliceAngle;
    currentAngle = endAngle;

    const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
    const largeArcFlag = sliceAngle > 180 ? 1 : 0;
    const radius = 500;

    const x1 = radius + radius * Math.cos((startAngle * Math.PI) / 180);
    const y1 = radius + radius * Math.sin((startAngle * Math.PI) / 180);
    const x2 = radius + radius * Math.cos((endAngle * Math.PI) / 180);
    const y2 = radius + radius * Math.sin((endAngle * Math.PI) / 180);

    path.setAttribute(
      "d",
      `M ${radius},${radius} L ${x1},${y1} A ${radius},${radius} 0 ${largeArcFlag},1 ${x2},${y2} Z`
    );
    path.setAttribute("fill", segment.color);

    const textAngle = startAngle + sliceAngle / 2;
    const textRadius = radius - 100;

    const textX = radius + textRadius * Math.cos((textAngle * Math.PI) / 180);
    const textY = radius + textRadius * Math.sin((textAngle * Math.PI) / 180);

    const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
    text.setAttribute("x", textX);
    text.setAttribute("y", textY);
    text.setAttribute("text-anchor", "middle");
    text.setAttribute("alignment-baseline", "middle");

    // Apply rotation for proper alignment
    const rotation = textAngle; // Rotate text to align perpendicularly

    // Reflect text horizontally if above center
    const reflect = textAngle > 90 && textAngle < 270 ? 180 : 0;

    text.setAttribute(
      "transform",
      `rotate(${rotation} ${textX} ${textY})`
    );

    text.textContent = segment.label;

    wheel.appendChild(path);
    wheel.appendChild(text);
  });
}



// Spin logic
function spinWheel() {
  if (isSpinning) return;

  isSpinning = true;
  spinButton.disabled = true;

  const spinAngle = Math.floor(Math.random() * 360) + 3600; // Random large spin
  currentRotation += spinAngle;
  wheel.style.transform = `rotate(${currentRotation}deg)`;

  // Play spin sound
  spinSound.play();

  // Show result after spin completes
  setTimeout(() => {
    const normalizedRotation = currentRotation % 360;
    let cumulativeAngle = 0;

    const resultIndex = segments.findIndex((segment) => {
      const segmentAngle = (segment.weight / segments.reduce((sum, s) => sum + s.weight, 0)) * 360;
      cumulativeAngle += segmentAngle;
      return normalizedRotation <= cumulativeAngle;
    });

    alert(`Your prize: ${segments[resultIndex].label}`);
    isSpinning = false;
    spinButton.disabled = false;
  }, 4000);
}

// Attach spin button event
spinButton.addEventListener("click", spinWheel);

// Initialize wheel
generateWheel();
