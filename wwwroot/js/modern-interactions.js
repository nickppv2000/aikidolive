// Modern Aikido Library Interactions
document.addEventListener('DOMContentLoaded', function() {
    // Initialize modern interactions
    initializeAnimations();
    initializeSearch();
    initializeResponsiveMenu();
    initializeScrollEffects();
});

// Animation observers
function initializeAnimations() {
    // Intersection Observer for fade-in animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Observe all fade-in elements
    document.querySelectorAll('.fade-in-up, .fade-in-up-delay').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px)';
        el.style.transition = 'opacity 0.6s ease-out, transform 0.6s ease-out';
        observer.observe(el);
    });
}

// Enhanced search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-modern');
    if (!searchInput) return;

    let searchTimeout;
    
    searchInput.addEventListener('input', function(e) {
        clearTimeout(searchTimeout);
        const query = e.target.value.toLowerCase().trim();
        
        searchTimeout = setTimeout(() => {
            performSearch(query);
        }, 300);
    });
    
    // Clear placeholder on focus
    searchInput.addEventListener('focus', function() {
        if (this.value === 'search') {
            this.value = '';
        }
    });
    
    // Restore placeholder if empty
    searchInput.addEventListener('blur', function() {
        if (this.value === '') {
            this.value = 'search';
        }
    });
}

function performSearch(query) {
    if (!query) {
        showAllContent();
        return;
    }
    
    // Search in library items
    const libraryItems = document.querySelectorAll('.library-item, .card');
    let visibleCount = 0;
    
    libraryItems.forEach(item => {
        const text = item.textContent.toLowerCase();
        const isVisible = text.includes(query);
        
        item.style.display = isVisible ? 'block' : 'none';
        if (isVisible) visibleCount++;
    });
    
    // Show search results feedback
    showSearchFeedback(query, visibleCount);
}

function showAllContent() {
    document.querySelectorAll('.library-item, .card').forEach(item => {
        item.style.display = 'block';
    });
    hideSearchFeedback();
}

function showSearchFeedback(query, count) {
    let feedback = document.querySelector('.search-feedback');
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'search-feedback card';
        feedback.style.marginTop = '1rem';
        document.querySelector('.content-area').insertBefore(feedback, document.querySelector('.content-area').firstChild);
    }
    
    feedback.innerHTML = `
        <div class="card-content">
            <p><strong>Search Results:</strong> Found ${count} items for "${query}"</p>
            ${count === 0 ? '<p>Try adjusting your search terms or browse all content.</p>' : ''}
        </div>
    `;
    feedback.style.display = 'block';
}

function hideSearchFeedback() {
    const feedback = document.querySelector('.search-feedback');
    if (feedback) {
        feedback.style.display = 'none';
    }
}

// Responsive menu handling
function initializeResponsiveMenu() {
    const menuToggle = document.querySelector('.mobile-menu-toggle');
    const navContainer = document.querySelector('.nav-container');
    
    if (!menuToggle || !navContainer) return;
      // Toggle mobile menu
    menuToggle.addEventListener('click', function() {
        const isActive = this.classList.contains('active');
        
        if (isActive) {
            // Close menu
            this.classList.remove('active');
            navContainer.classList.remove('active');
            this.setAttribute('aria-expanded', 'false');
            document.body.classList.remove('mobile-menu-open');
        } else {
            // Open menu
            this.classList.add('active');
            navContainer.classList.add('active');
            this.setAttribute('aria-expanded', 'true');
            document.body.classList.add('mobile-menu-open');
        }
    });
      // Close menu when clicking on a nav link
    const navLinks = document.querySelectorAll('.nav-modern a');
    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            menuToggle.classList.remove('active');
            navContainer.classList.remove('active');
            menuToggle.setAttribute('aria-expanded', 'false');
            document.body.classList.remove('mobile-menu-open');
        });
    });
    
    // Close menu when clicking outside
    document.addEventListener('click', function(event) {
        if (!menuToggle.contains(event.target) && !navContainer.contains(event.target)) {
            menuToggle.classList.remove('active');
            navContainer.classList.remove('active');
            menuToggle.setAttribute('aria-expanded', 'false');
            document.body.classList.remove('mobile-menu-open');
        }
    });
    
    // Handle window resize
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            // Reset mobile menu state on desktop
            menuToggle.classList.remove('active');
            navContainer.classList.remove('active');
            menuToggle.setAttribute('aria-expanded', 'false');
            document.body.classList.remove('mobile-menu-open');
        }
    });
    
    // Handle escape key
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape' && navContainer.classList.contains('active')) {
            menuToggle.classList.remove('active');
            navContainer.classList.remove('active');
            menuToggle.setAttribute('aria-expanded', 'false');
            document.body.classList.remove('mobile-menu-open');
        }
    });
}

// Scroll effects
function initializeScrollEffects() {
    let lastScrollTop = 0;
    const header = document.querySelector('.modern-header');
    
    if (!header) return;
    
    window.addEventListener('scroll', function() {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        
        // Add shadow to header on scroll
        if (scrollTop > 0) {
            header.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)';
        } else {
            header.style.boxShadow = '0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24)';
        }
        
        lastScrollTop = scrollTop <= 0 ? 0 : scrollTop;
    });
}

// Enhanced card interactions
document.addEventListener('DOMContentLoaded', function() {
    // Add hover effects to cards
    document.querySelectorAll('.card, .library-item').forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
    
    // Enhanced menu link interactions
    document.querySelectorAll('.menu-link').forEach(link => {
        link.addEventListener('mouseenter', function() {
            this.style.transform = 'translateX(5px)';
        });
        
        link.addEventListener('mouseleave', function() {
            this.style.transform = 'translateX(0)';
        });
    });
});

// Loading states for dynamic content
function showLoading(element) {
    if (!element) return;
    element.classList.add('loading');
    element.style.position = 'relative';
}

function hideLoading(element) {
    if (!element) return;
    element.classList.remove('loading');
}

// Smooth scrolling for anchor links
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});

// Form enhancements
function enhanceForms() {
    document.querySelectorAll('input, select, textarea').forEach(input => {
        // Add focus effects
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        input.addEventListener('blur', function() {
            this.parentElement.classList.remove('focused');
        });
        
        // Add validation feedback
        input.addEventListener('invalid', function() {
            this.style.borderColor = 'var(--accent-color)';
        });
        
        input.addEventListener('input', function() {
            if (this.validity.valid) {
                this.style.borderColor = 'var(--success-color)';
            }
        });
    });
}

// Initialize form enhancements
document.addEventListener('DOMContentLoaded', enhanceForms);

// Initialize responsive menu
document.addEventListener('DOMContentLoaded', initializeResponsiveMenu);

// Initialize scroll effects
document.addEventListener('DOMContentLoaded', initializeScrollEffects);

// Accessibility improvements
document.addEventListener('DOMContentLoaded', function() {
    // Add skip link
    const skipLink = document.createElement('a');
    skipLink.href = '#main-content';
    skipLink.className = 'skip-link';
    skipLink.textContent = 'Skip to main content';
    skipLink.style.cssText = `
        position: absolute;
        top: -40px;
        left: 6px;
        background: var(--primary-color);
        color: white;
        padding: 8px;
        text-decoration: none;
        z-index: 1000;
        transition: top 0.3s;
    `;
    
    skipLink.addEventListener('focus', function() {
        this.style.top = '6px';
    });
    
    skipLink.addEventListener('blur', function() {
        this.style.top = '-40px';
    });
    
    document.body.insertBefore(skipLink, document.body.firstChild);
    
    // Add main content ID if not present
    const mainContent = document.querySelector('main, .content-area');
    if (mainContent && !mainContent.id) {
        mainContent.id = 'main-content';
    }
});

// Performance optimization
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Export functions for use in other scripts
window.AikidoLibrary = {
    showLoading,
    hideLoading,
    performSearch,
    showAllContent,
    debounce
};
