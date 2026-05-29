# ☕ Say Tea Coffee - Franchise POS & Management System

A multi-tier Windows desktop application engineered for F&B franchise management. This project demonstrates **Object-Oriented Programming (OOP)** principles, separation of concerns (UI vs. Business Logic), and strict database transaction management.


## 🚀 Key Technical Highlights
* **HMI & UI Design:** Developed using **C# WPF** and **XAML**. Implemented `ObservableCollection` for real-time Data Binding and state management without performance degradation.
* **Database & ACID Transactions:** Architected a **MySQL** relational database. Applied strict transaction structures (`BEGIN`, `COMMIT`, `ROLLBACK`) to ensure 100% data integrity during concurrent order processing and automated inventory deductions.
* **OOP Architecture:** Utilized Interface implementation, Abstraction, and Polymorphism to handle dynamic UI rendering and multi-window routing.

## ⚙️ Prerequisites for Running
To test this application locally, you must have the following installed:
1. **Windows OS** with .NET Desktop Runtime (v6.0 or higher).
2. **MySQL Server** (or XAMPP/WAMP) running on port `3306`.

## 🛠️ Installation & Setup Guide

### Step 1: Database Setup
1. Open MySQL Workbench or phpMyAdmin.
2. Create a new schema named `SayTeaCoffee`.
3. Import the database dump file located in this repository: `Database/Database_SayTeaCoffee.sql`.

### Step 2: Configure Connection String
If your MySQL password is not `123456`, you will need to update the connection string in the source code before compiling.
Locate the `connectionString` variable in the following files and update your `Uid` and `Pwd`:
* `MainWindow.xaml.cs`
* `PosWindow.xaml.cs`
* `AdminWindow.xaml.cs`

### Step 3: Build & Run
1. Open **Visual Studio** (2022 or higher recommended).
2. Choose **Open a project or solution** and select the solution file: `TraSuaApp_New.sln`.
3. Make sure the build configuration dropdown on the top toolbar is set to **Debug** (for inspecting code) or **Release** (for maximum performance).
4. Press **F5** (Start Debugging) or **Ctrl + F5** (Start Without Debugging) to automatically compile, build, and launch the application.

## 🔑 Default Test Accounts
After running the app, use the following credentials to log in:
* **Admin / Manager Role:**
  * Username: `admin`
  * Password: `123`
* **Staff Role (POS System):**
  * Username: *(Create one via the Admin dashboard)*

---
*Created by [Your Name] - Electronics & Embedded Systems Engineering.*
