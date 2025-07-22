# 🎲 Task 3: Non-Transitive Dice Game with HMAC

This is a CLI-based C# game that simulates non-transitive dice play between a user and the computer, using cryptographic HMAC to ensure fairness.

---

## ✅ Features

- ✔️ Accepts 3 or more dice as command-line arguments (e.g., `"2,2,4,4,9,9" "6,8,1,1,8,6" "7,5,3,7,5,3"`)
- ✔️ Validates arguments to ensure correct dice format
- ✔️ Commits to a **random first-move bit (0 or 1)** using **HMAC-SHA256** before player sees it
- ✔️ Displays an interactive menu:
  - Select a die
  - Help option
  - Exit option
- ✔️ Computer/player choose dice based on first-move bit
- ✔️ Reveals HMAC key and bit to verify integrity
- ✔️ Simulates dice roll and declares a winner

---

## 📸 Demo Video

▶️ Watch the demo: (https://youtu.be/snSrPxtoYrk)

---

## 🚀 How to Run

Make sure you have [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.

```bash
dotnet run -- "2,2,4,4,9,9" "6,8,1,1,8,6" "7,5,3,7,5,3"
