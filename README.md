# Project Elysium

![Technology](https://img.shields.io/badge/backend-ASP.NET%20Core-512BD4)
![Technology](https://img.shields.io/badge/frontend-Vanilla%20JS-F7DF1E)
![Status](https://img.shields.io/badge/status-live%20%26%20in%20progress-brightgreen)

**Elysium** is an intelligent character generator for vampire tabletop rpg.

Elysium uses a weighted logic system to ensure characters are mechanically valid, thematically consistent, and ready for play. It bridges the gap between randomization and coherent storytelling.

### 🔗 Live Demo
Try the generator here: **[elysium.mustafaguler.me](https://elysium.mustafaguler.me)**

## ✨ Key Features

*   **Thematic Consistency:** The generator prioritizes Attributes, Abilities, and Disciplines that make sense for the character's Clan and selected Concept.
*   **V20 Ruleset Compliance:** Handles generation limits, blood pools, and trait costs according to the 20th rules.
*   **Smart Spending:** Simulates a player spending Freebie Points and Experience Points (XP) based on character age and archetype.
*   **PDF Export:** Instantly generates a filled, ready-to-print PDF character sheet via a [Python Microservice](https://github.com/MustafaGuler98/vtm-scribe-service). *(Special thanks to Mr.Gone)*
*   **Customization:** Allows users to lock specific traits (Clan, Nature, Age) while randomizing the rest.

## 🛠 Tech Stack

*   **Backend:** C# / ASP.NET Core 8 Web API
*   **Frontend:** Vanilla JavaScript
*   **Microservice:** Python / FastAPI (PDF manipulation)

## 🤝 Feedback

This project is currently in **Beta**. If you encounter logical errors, bugs, or have suggestions, please reach out:

📧 [contact.mustafaguler@gmail.com](mailto:contact.mustafaguler@gmail.com)