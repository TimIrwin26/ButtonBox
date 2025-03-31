#include <stdio.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/gpio.h"

// Define GPIO pins for the buttons
#define BUTTON1_GPIO 0
#define BUTTON2_GPIO 1

void app_main() {
    // Configure GPIO pins as input
    gpio_config_t io_conf;
    io_conf.intr_type = GPIO_INTR_DISABLE; // Disable interrupts
    io_conf.mode = GPIO_MODE_INPUT;        // Set as input mode
    io_conf.pin_bit_mask = (1ULL << BUTTON1_GPIO) | (1ULL << BUTTON2_GPIO); // Select pins
    io_conf.pull_down_en = GPIO_PULLDOWN_DISABLE; // Disable pull-down
    io_conf.pull_up_en = GPIO_PULLUP_ENABLE;     // Enable pull-up
    gpio_config(&io_conf);

    // Initialize serial communication
    printf("ESP32 Button Press Reporter\n");

    while (1) {
        // Read button states
        int button1_state = gpio_get_level(BUTTON1_GPIO);
        int button2_state = gpio_get_level(BUTTON2_GPIO);

        // Check which button is pressed and print to serial
        if (button1_state == 0) { // Button 1 pressed (active low)
            printf("1\n");
            vTaskDelay(pdMS_TO_TICKS(200)); // Debounce delay
        }
        if (button2_state == 0) { // Button 2 pressed (active low)
            printf("2\n");
            vTaskDelay(pdMS_TO_TICKS(200)); // Debounce delay
        }

        // Small delay to avoid busy looping
        vTaskDelay(pdMS_TO_TICKS(50));
    }
}