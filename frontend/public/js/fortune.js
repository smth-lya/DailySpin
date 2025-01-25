// Reward Popup
const rewardPopup = document.getElementById('reward-popup');
const rewardText = document.getElementById('reward-text');
const closePopup = document.getElementById('close-popup');

document.getElementById('spin-btn').addEventListener('click', () => {
    const rewards = ['+50 XP', 'Golden Key', '+100 XP', 'Extra Spin'];
    const randomReward = rewards[Math.floor(Math.random() * rewards.length)];
    rewardText.textContent = randomReward;

    rewardPopup.classList.add('visible');
});

closePopup.addEventListener('click', () => {
    rewardPopup.classList.remove('visible');
});
