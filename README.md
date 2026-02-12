# LibraryManagementSystem
1.git clone https://github.com/sandeepverma2002/LibraryManagementSystem.git
2.cd LibraryManagementSystem
3.code .

<!-- then first create mysql database -->
4.CREATE DATABASE IF NOT EXISTS LibraryDB;

5.<!-- open appsetting.json and enter mysq port,password,databasename,userame -->

   "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=LibraryDB;user=root;password=1234;"
  },


<!--  want to mysq query -->
<!-- -- Create Database -->
6.CREATE DATABASE IF NOT EXISTS LibraryDB;
USE LibraryDB;

-- Books Table
CREATE TABLE Books (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Title VARCHAR(200) NOT NULL,
    Author VARCHAR(150) NOT NULL,
    ISBN VARCHAR(20) UNIQUE NOT NULL,
    Publisher VARCHAR(150),
    PublishedYear INT,
    Genre VARCHAR(100),
    TotalCopies INT DEFAULT 1,
    AvailableCopies INT DEFAULT 1,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Users Table (Library Members)
CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) UNIQUE NOT NULL,
    Phone VARCHAR(20),
    MembershipNumber VARCHAR(50) UNIQUE NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Transactions Table (Issues/Returns)
CREATE TABLE Transactions (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    BookId INT NOT NULL,
    UserId INT NOT NULL,
    IssueDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status ENUM('Issued', 'Returned', 'Overdue') DEFAULT 'Issued',
    FineAmount DECIMAL(10,2) DEFAULT 0.00,
    FOREIGN KEY (BookId) REFERENCES Books(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Insert Sample Data
INSERT INTO Books
(Title, Author, ISBN, Publisher, PublishedYear, Genre,
 TotalCopies, AvailableCopies, CreatedDate, UpdatedDate)
VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', 'Scribner', 1925, 'Classic', 5, 5, NOW(), NOW()),
('To Kill a Mockingbird', 'Harper Lee', '9780446310789', 'J.B. Lippincott', 1960, 'Fiction', 3, 3, NOW(), NOW()),
('1984', 'George Orwell', '9780451524935', 'Secker & Warburg', 1949, 'Dystopian', 4, 4, NOW(), NOW());


INSERT INTO Users 
(FirstName, LastName, Email, Phone, MembershipNumber, CreatedDate)
VALUES
('sandeep', 'kumar', 'sandeep@email.com', '1234567890', 'MEM001', NOW()),
('Jane', 'Smith', 'jane.smith@email.com', '9876543210', 'MEM002', NOW());



<!-- #start project  -->
7.open terminal run these comands
 <!-- Restore packages -->
dotnet restore 
 <!-- to run project -->
 dotnet run


<!-- it run on  -->
http://localhost:5063





