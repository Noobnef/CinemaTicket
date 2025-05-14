// Optional: Add some interactivity
document.addEventListener('DOMContentLoaded', () => {
    const poster = document.querySelector('.movie-poster img');
    poster.addEventListener('mouseenter', (e) => {
        e.target.style.transform = 'scale(1.05)';
    });
    poster.addEventListener('mouseleave', (e) => {
        e.target.style.transform = 'scale(1)';
    });
});