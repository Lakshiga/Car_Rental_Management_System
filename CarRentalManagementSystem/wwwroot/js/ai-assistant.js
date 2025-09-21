// AI Assistant JavaScript Functionality
class AIAssistant {
    constructor() {
        this.isOpen = false;
        this.isTyping = false;
        this.userInfo = this.getUserInfo();
        this.userType = this.userInfo.type;
        this.userId = this.userInfo.id;
        this.apiEndpoint = this.getApiEndpoint();
        this.storageKey = this.getStorageKey();
        
        this.initializeEventListeners();
        // Don't load chat history - start fresh each time
        this.clearChatHistory();
    }

    getUserInfo() {
        // Get user information from meta tags
        const customerId = document.querySelector('meta[name="customer-id"]')?.content;
        const userRole = document.querySelector('meta[name="user-role"]')?.content;
        const userId = document.querySelector('meta[name="user-id"]')?.content;
        const staffId = document.querySelector('meta[name="staff-id"]')?.content;
        
        // Determine user type and ID
        if (customerId && customerId !== '') {
            return {
                type: 'customer',
                id: customerId,
                role: 'Customer'
            };
        } else if (userRole === 'Admin') {
            return {
                type: 'admin',
                id: userId || 'admin',
                role: userRole
            };
        } else if (userRole === 'Staff') {
            return {
                type: 'staff',
                id: staffId || 'staff',
                role: userRole
            };
        } else {
            return {
                type: 'guest',
                id: 'guest',
                role: 'Guest'
            };
        }
    }

    getApiEndpoint() {
        switch(this.userType) {
            case 'customer':
                return '/api/aiassistant/customer';
            case 'admin':
                return '/api/aiassistant/admin';
            case 'staff':
                return '/api/aiassistant/staff';
            case 'guest':
                return '/api/aiassistant/guest';
            default:
                return '/api/aiassistant/guest'; // Default to guest endpoint
        }
    }

    getStorageKey() {
        // Create unique storage key based on user type and ID
        return `ai-assistant-chat-${this.userType}-${this.userId}`;
    }

    initializeEventListeners() {
        // Float button click
        const floatBtn = document.getElementById('ai-assistant-float-btn');
        if (floatBtn) {
            floatBtn.addEventListener('click', () => this.toggleChat());
        }

        // Close button click
        const closeBtn = document.getElementById('ai-assistant-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', () => this.closeChat());
        }

        // Send button click
        const sendBtn = document.getElementById('ai-assistant-send');
        if (sendBtn) {
            sendBtn.addEventListener('click', () => this.sendMessage());
        }

        // Enter key press
        const input = document.getElementById('ai-assistant-input');
        if (input) {
            input.addEventListener('keypress', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    this.sendMessage();
                }
            });

            // Auto-resize input
            input.addEventListener('input', () => this.autoResizeInput());
        }

        // Quick action buttons
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('quick-action-btn') || e.target.closest('.quick-action-btn')) {
                const btn = e.target.classList.contains('quick-action-btn') ? e.target : e.target.closest('.quick-action-btn');
                const message = btn.getAttribute('data-message');
                if (message) {
                    this.sendPredefinedMessage(message);
                }
            }
        });

        // Close modal when clicking outside
        const modal = document.getElementById('ai-assistant-modal');
        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    this.closeChat();
                }
            });
        }

        // Escape key to close
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isOpen) {
                this.closeChat();
            }
        });
    }

    toggleChat() {
        if (this.isOpen) {
            this.closeChat();
        } else {
            this.openChat();
        }
    }

    openChat() {
        const modal = document.getElementById('ai-assistant-modal');
        const floatBtn = document.getElementById('ai-assistant-float-btn');
        
        if (modal && floatBtn) {
            modal.style.display = 'block';
            setTimeout(() => {
                modal.classList.add('show');
            }, 10);
            
            // Hide float button
            floatBtn.style.transform = 'scale(0)';
            
            // Focus on input
            const input = document.getElementById('ai-assistant-input');
            if (input) {
                setTimeout(() => input.focus(), 300);
            }
            
            this.isOpen = true;
            
            // Scroll to bottom of messages
            this.scrollToBottom();
        }
    }

    closeChat() {
        const modal = document.getElementById('ai-assistant-modal');
        const floatBtn = document.getElementById('ai-assistant-float-btn');
        
        if (modal && floatBtn) {
            modal.classList.remove('show');
            setTimeout(() => {
                modal.style.display = 'none';
            }, 300);
            
            // Show float button
            floatBtn.style.transform = 'scale(1)';
            
            this.isOpen = false;
        }
    }

    async sendMessage() {
        const input = document.getElementById('ai-assistant-input');
        const sendBtn = document.getElementById('ai-assistant-send');
        
        if (!input || this.isTyping) return;
        
        const message = input.value.trim();
        if (!message) return;
        
        // Clear input and disable send button
        input.value = '';
        this.toggleSendButton(false);
        
        // Add user message to chat
        this.addUserMessage(message);
        
        try {
            // Show typing indicator
            this.showTypingIndicator();
            
            console.log('Sending message to:', this.apiEndpoint);
            console.log('Message:', message);
            
            const response = await fetch(this.apiEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message: message })
            });
            
            console.log('Response status:', response.status);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            console.log('Response data:', data);
            
            // Hide typing indicator
            this.hideTypingIndicator();
            
            if (data.success) {
                this.addAIMessage(data.response);
            } else {
                console.error('API returned error:', data.errorMessage);
                this.addAIMessage(data.response || 'Sorry, I encountered an error. Please try again.');
            }
            
        } catch (error) {
            console.error('Error sending message:', error);
            this.hideTypingIndicator();
            this.addAIMessage('I apologize, but I\'m having trouble connecting right now. Please check the console for more details and try again in a moment.');
        } finally {
            this.toggleSendButton(true);
            input.focus();
        }
    }

    sendPredefinedMessage(message) {
        const input = document.getElementById('ai-assistant-input');
        if (input) {
            input.value = message;
            this.sendMessage();
        }
    }

    addUserMessage(message, save = false) {
        const messagesContainer = document.getElementById('ai-assistant-messages');
        if (!messagesContainer) return;
        
        const messageElement = this.createMessageElement('user', message);
        messagesContainer.appendChild(messageElement);
        this.scrollToBottom();
        
        // Don't save to localStorage - sessions should be isolated
    }

    addAIMessage(message, save = false) {
        const messagesContainer = document.getElementById('ai-assistant-messages');
        if (!messagesContainer) return;
        
        const messageElement = this.createMessageElement('ai', message);
        messagesContainer.appendChild(messageElement);
        this.scrollToBottom();
        
        // Don't save to localStorage - sessions should be isolated
    }

    createMessageElement(type, message) {
        const messageDiv = document.createElement('div');
        messageDiv.className = type === 'user' ? 'user-message' : 'ai-message';
        
        const currentTime = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        
        messageDiv.innerHTML = `
            <div class="message-avatar">
                <i class="fas fa-${type === 'user' ? 'user' : 'robot'}"></i>
            </div>
            <div class="message-content">
                <p>${this.formatMessage(message)}</p>
                <small class="message-time">${currentTime}</small>
            </div>
        `;
        
        return messageDiv;
    }

    formatMessage(message) {
        // Convert line breaks to <br> tags
        message = message.replace(/\n/g, '<br>');
        
        // Make URLs clickable
        const urlRegex = /(https?:\/\/[^\s]+)/g;
        message = message.replace(urlRegex, '<a href="$1" target="_blank" rel="noopener noreferrer">$1</a>');
        
        // Make bold text work
        message = message.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        
        return message;
    }

    showTypingIndicator() {
        const indicator = document.getElementById('ai-typing-indicator');
        if (indicator) {
            indicator.style.display = 'flex';
            this.isTyping = true;
            this.scrollToBottom();
        }
    }

    hideTypingIndicator() {
        const indicator = document.getElementById('ai-typing-indicator');
        if (indicator) {
            indicator.style.display = 'none';
            this.isTyping = false;
        }
    }

    toggleSendButton(enabled) {
        const sendBtn = document.getElementById('ai-assistant-send');
        if (sendBtn) {
            sendBtn.disabled = !enabled;
        }
    }

    scrollToBottom() {
        const messagesContainer = document.getElementById('ai-assistant-messages');
        if (messagesContainer) {
            setTimeout(() => {
                messagesContainer.scrollTop = messagesContainer.scrollHeight;
            }, 100);
        }
    }

    autoResizeInput() {
        const input = document.getElementById('ai-assistant-input');
        if (input) {
            input.style.height = 'auto';
            input.style.height = Math.min(input.scrollHeight, 120) + 'px';
        }
    }

    saveChatHistory() {
        // Chat history is not saved - each session starts fresh
        // This ensures proper isolation between roles and sessions
    }

    loadChatHistory() {
        // Chat history is not loaded - each session starts fresh
        // This ensures proper isolation between roles and sessions
        // Only the welcome message will be displayed
    }

    clearChatHistory() {
        // Clear any existing localStorage for this user
        localStorage.removeItem(this.storageKey);
        
        const messagesContainer = document.getElementById('ai-assistant-messages');
        if (messagesContainer) {
            // Keep only welcome message
            const welcomeMsg = messagesContainer.querySelector('.welcome-message');
            messagesContainer.innerHTML = '';
            if (welcomeMsg) {
                messagesContainer.appendChild(welcomeMsg);
            }
        }
    }

    // Method to get contextual help based on current page
    async getContextualHelp() {
        try {
            const currentPage = window.location.pathname.split('/').pop() || 'dashboard';
            let contextEndpoint = '/api/aiassistant/context/guest';
            
            if (this.userType === 'customer') {
                contextEndpoint = '/api/aiassistant/context/customer';
            } else if (this.userType === 'admin') {
                contextEndpoint = '/api/aiassistant/context/admin';
            } else if (this.userType === 'staff') {
                contextEndpoint = '/api/aiassistant/context/staff';
            }
            
            const response = await fetch(`${contextEndpoint}?page=${currentPage}`);
            const data = await response.json();
            
            if (data.context) {
                this.addAIMessage(`Here's some contextual help for the current page:\n\n${data.context}`);
            }
        } catch (error) {
            console.error('Error getting contextual help:', error);
        }
    }
}

// Global function to clear all AI chat histories (for complete reset)
function clearAllAIChats() {
    const keys = Object.keys(localStorage);
    keys.forEach(key => {
        if (key.startsWith('ai-assistant-chat-')) {
            localStorage.removeItem(key);
        }
    });
    console.log('Cleared all AI chat histories');
    if (window.aiAssistant) {
        window.aiAssistant.clearChatHistory();
    }
}

// Function to clear chat histories when user role changes
function clearChatOnRoleChange() {
    // Clear all existing chat histories to ensure role isolation
    clearAllAIChats();
    
    // Reinitialize the assistant if it exists
    if (window.aiAssistant) {
        window.aiAssistant = new AIAssistant();
    }
}

// Function to be called on logout to clear chat histories
function clearChatOnLogout() {
    clearAllAIChats();
}

// Auto-clear on page load to ensure fresh start
document.addEventListener('DOMContentLoaded', function() {
    // Clear any existing chat histories to ensure fresh start
    const keys = Object.keys(localStorage);
    keys.forEach(key => {
        if (key.startsWith('ai-assistant-chat-')) {
            localStorage.removeItem(key);
        }
    });
});

// Initialize AI Assistant when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Check if AI Assistant elements exist
    const floatBtn = document.getElementById('ai-assistant-float-btn');
    if (floatBtn) {
        window.aiAssistant = new AIAssistant();
        
        // Add contextual help option
        const quickActions = document.querySelector('.ai-assistant-quick-actions');
        if (quickActions) {
            const helpBtn = document.createElement('button');
            helpBtn.className = 'quick-action-btn';
            helpBtn.innerHTML = '<i class="fas fa-question-circle"></i> Help';
            helpBtn.addEventListener('click', () => {
                window.aiAssistant.getContextualHelp();
            });
            quickActions.appendChild(helpBtn);
        }
    }
});