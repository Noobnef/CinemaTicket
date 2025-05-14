document.addEventListener('DOMContentLoaded', function () {
    // Toggle Dark Mode
    const toggleBtn = document.querySelector('.mode-toggle');

    if (toggleBtn) {
        toggleBtn.addEventListener("click", function () {
            document.body.classList.toggle("dark-mode");

            const icon = this.querySelector('i');
            const isDark = document.body.classList.contains("dark-mode");

            this.classList.toggle("btn-dark", !isDark);
            this.classList.toggle("btn-light", isDark);

            icon.classList.toggle("fa-moon", !isDark);
            icon.classList.toggle("fa-sun", isDark);

            localStorage.setItem("darkMode", isDark ? "enabled" : "disabled");
        });

        const savedMode = localStorage.getItem("darkMode");
        if (savedMode === "enabled") {
            document.body.classList.add("dark-mode");

            toggleBtn.classList.remove("btn-dark");
            toggleBtn.classList.add("btn-light");

            const icon = toggleBtn.querySelector('i');
            icon.classList.remove("fa-moon");
            icon.classList.add("fa-sun");
        }
    }

    // Initialize Swiper
    const swiper = new Swiper('.movie-swiper', {
        loop: true,
        loopedSlides: 4,
        slidesPerView: 1,
        spaceBetween: 10,
        autoplay: {
            delay: 3000,
            disableOnInteraction: false,
        },
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
        },
        navigation: {
            nextEl: '.swiper-button-next',
            prevEl: '.swiper-button-prev',
        },
        breakpoints: {
            480: { slidesPerView: 1 },
            768: { slidesPerView: 2, spaceBetween: 20 },
            1024: { slidesPerView: 4, spaceBetween: 30 }
        }
    });

    // Seat selection logic
    const seats = document.querySelectorAll('.seat:not(.occupied)');
    const selectedSeatsElement = document.getElementById('selectedSeats');
    const ticketCountElement = document.getElementById('ticketCount');
    const totalPriceElement = document.getElementById('totalPrice');
    const hiddenSeatsInput = document.getElementById('hiddenSeats');
    const hiddenAmountInput = document.getElementById('hiddenAmount');
    const checkoutBtn = document.getElementById('checkout');
    const bookingProgressBar = document.getElementById('bookingProgress');

    let selectedSeats = [];
    const ticketPrice = 100000;

    seats.forEach(seat => {
        seat.addEventListener('click', function () {
            this.classList.toggle('selected');
            const seatNumber = this.getAttribute('data-seat');

            if (this.classList.contains('selected')) {
                selectedSeats.push(seatNumber);
            } else {
                const index = selectedSeats.indexOf(seatNumber);
                if (index > -1) selectedSeats.splice(index, 1);
            }

            updateSelectedSeats();
            updateBookingProgress();
        });
    });

    // Movie selection logic
    const movieButtons = document.querySelectorAll('.select-movie');
    const selectedMovieElement = document.getElementById('selectedMovie');

    movieButtons.forEach(button => {
        button.addEventListener('click', function () {
            const movieName = this.getAttribute('data-movie-name');
            selectedMovieElement.textContent = movieName;

            document.getElementById('seat-selection').scrollIntoView({
                behavior: 'smooth'
            });

            updateBookingProgress();
        });
    });

    // Time selection logic
    const timeButtons = document.querySelectorAll('.time-btn');
    const selectedTimeElement = document.getElementById('selectedTime');

    timeButtons.forEach(button => {
        button.addEventListener('click', function () {
            timeButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            const time = this.getAttribute('data-time');
            selectedTimeElement.textContent = time;

            updateBookingProgress();
        });
    });

    function updateSelectedSeats() {
        selectedSeatsElement.textContent = selectedSeats.length ? selectedSeats.join(', ') : 'Chưa chọn';
        ticketCountElement.textContent = selectedSeats.length;
        totalPriceElement.textContent = formatCurrency(selectedSeats.length * ticketPrice);

        hiddenSeatsInput.value = selectedSeats.join(',');
        hiddenAmountInput.value = selectedSeats.length * ticketPrice;

        checkoutBtn.disabled =
            selectedSeats.length === 0 ||
            selectedMovieElement.textContent === 'Chưa chọn' ||
            selectedTimeElement.textContent === 'Chưa chọn';
    }

    function updateBookingProgress() {
        let progress = 0;
        if (selectedMovieElement.textContent !== 'Chưa chọn') progress += 33;
        if (selectedTimeElement.textContent !== 'Chưa chọn') progress += 33;
        if (selectedSeats.length > 0) progress += 34;

        bookingProgressBar.style.width = progress + '%';
        bookingProgressBar.textContent = progress + '%';
        bookingProgressBar.setAttribute('aria-valuenow', progress);

        if (progress < 33) {
            bookingProgressBar.className = 'progress-bar bg-danger';
        } else if (progress < 66) {
            bookingProgressBar.className = 'progress-bar bg-warning';
        } else {
            bookingProgressBar.className = 'progress-bar bg-success';
        }
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ';
    }
});
