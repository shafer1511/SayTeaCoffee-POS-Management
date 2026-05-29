-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: sayteacoffee
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `accounts`
--

DROP TABLE IF EXISTS `accounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accounts` (
  `username` varchar(50) NOT NULL,
  `password` varchar(50) NOT NULL,
  `role` varchar(20) DEFAULT NULL,
  `store_id` varchar(10) DEFAULT NULL,
  `full_name` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`username`),
  KEY `store_id` (`store_id`),
  CONSTRAINT `accounts_ibfk_1` FOREIGN KEY (`store_id`) REFERENCES `stores` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `accounts`
--

LOCK TABLES `accounts` WRITE;
/*!40000 ALTER TABLE `accounts` DISABLE KEYS */;
INSERT INTO `accounts` VALUES ('admin','123','ADMIN','S01',NULL),('hehe','123','MANAGER','S02',NULL),('khoa','123','MANAGER','S03',NULL),('long','123','MANAGER','S05',NULL),('thungan','123','STAFF','S01','Lê Thu Ngân'),('tuan','123','MANAGER','S04',NULL);
/*!40000 ALTER TABLE `accounts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `branch_inventory`
--

DROP TABLE IF EXISTS `branch_inventory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `branch_inventory` (
  `store_id` varchar(10) NOT NULL,
  `product_id` varchar(10) NOT NULL,
  `stock` int DEFAULT '0',
  `sold_count` int DEFAULT '0',
  PRIMARY KEY (`store_id`,`product_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `branch_inventory_ibfk_1` FOREIGN KEY (`store_id`) REFERENCES `stores` (`id`) ON DELETE CASCADE,
  CONSTRAINT `branch_inventory_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `branch_inventory`
--

LOCK TABLES `branch_inventory` WRITE;
/*!40000 ALTER TABLE `branch_inventory` DISABLE KEYS */;
INSERT INTO `branch_inventory` VALUES ('S01','D01',50,0),('S01','D02',100,0),('S01','D03',58,0),('S01','D04',99,0),('S01','D05',100,0),('S01','D06',90,0),('S01','D07',100,0),('S01','D08',100,0),('S01','D09',100,0),('S01','D10',100,0),('S01','D11',100,0),('S01','D12',100,0),('S01','D13',100,0),('S01','D14',100,0),('S01','T01',190,0),('S01','T02',98,0),('S01','T03',100,0),('S01','T04',100,0),('S01','T05',100,0),('S01','T06',100,0),('S01','T07',99,0),('S01','T08',99,0),('S02','D04',100,0),('S02','D05',100,0),('S02','D06',100,0),('S02','D07',100,0),('S02','D08',100,0),('S02','D09',100,0),('S02','D10',100,0),('S02','D11',100,0),('S02','D12',90,0),('S02','D13',100,0),('S02','D14',100,0),('S02','T03',100,0),('S02','T04',100,0),('S02','T05',100,0),('S02','T06',100,0),('S02','T07',100,0),('S02','T08',100,0);
/*!40000 ALTER TABLE `branch_inventory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `id` varchar(10) NOT NULL,
  `store_id` varchar(10) DEFAULT NULL,
  `full_name` varchar(100) DEFAULT NULL,
  `shift` varchar(50) DEFAULT NULL,
  `work_hours` varchar(50) DEFAULT NULL,
  `is_clocked_in` tinyint(1) DEFAULT '0',
  `job_role` varchar(50) DEFAULT 'Phục vụ',
  PRIMARY KEY (`id`),
  KEY `store_id` (`store_id`),
  CONSTRAINT `employees_ibfk_1` FOREIGN KEY (`store_id`) REFERENCES `stores` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employees`
--

LOCK TABLES `employees` WRITE;
/*!40000 ALTER TABLE `employees` DISABLE KEYS */;
INSERT INTO `employees` VALUES ('NV001','S01','Nguyễn Pha Chế','Ca Sáng','06:00 - 11:00',0,'Pha chế'),('NV002','S01','Trần Phục Vụ','Ca Chiều','12:00 - 17:00',0,'Phục vụ'),('NV003','S01','Lê Thu Ngân','Ca Tối','17:00 - 22:00',0,'Thu ngân');
/*!40000 ALTER TABLE `employees` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `invoice_details`
--

DROP TABLE IF EXISTS `invoice_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoice_details` (
  `id` int NOT NULL AUTO_INCREMENT,
  `invoice_id` varchar(20) NOT NULL,
  `product_id` varchar(20) NOT NULL,
  `product_name` varchar(100) NOT NULL,
  `size` varchar(10) DEFAULT NULL,
  `sugar` varchar(10) DEFAULT NULL,
  `ice` varchar(10) DEFAULT NULL,
  `quantity` int NOT NULL,
  `unit_price` int NOT NULL,
  `subtotal` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `invoice_id` (`invoice_id`),
  CONSTRAINT `invoice_details_ibfk_1` FOREIGN KEY (`invoice_id`) REFERENCES `invoices` (`invoice_id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoice_details`
--

LOCK TABLES `invoice_details` WRITE;
/*!40000 ALTER TABLE `invoice_details` DISABLE KEYS */;
INSERT INTO `invoice_details` VALUES (1,'HD260411222313','D01','Trà Sữa Matcha','S','100%','100%',50,30000,1500000),(2,'HD260411222313','D03','Sữa Tươi Trân Châu','S','100%','100%',40,25000,1000000),(3,'HD260411222313','T01','Trân Châu Đen','-','-','-',10,7000,70000),(4,'HD260411222313','T02','Trân Châu Trắng','-','-','-',100,7000,700000),(5,'HD260411224132','D12','Bạc Xỉu','L','50%','100%',10,39000,390000),(6,'HD260411225153','D06','Trà Sữa Oolong Nướng','L','50%','100%',10,50000,500000),(7,'HD260413222224','D04','Trà Sữa Truyền Thống','S','100%','100%',1,30000,30000),(8,'HD260413222224','D03','Sữa Tươi Trân Châu','S','100%','100%',1,25000,25000),(9,'HD260413222224','T02','Trân Châu Trắng','-','-','-',1,7000,7000),(10,'HD260413222224','T07','Khúc Bạch','-','-','-',1,12000,12000),(11,'HD260413222224','T08','Hạt Sen Bùi','-','-','-',1,10000,10000),(12,'HD260415213734','D03','Sữa Tươi Trân Châu','S','100%','100%',1,25000,25000),(13,'HD260415213734','T02','Trân Châu Trắng','-','-','-',1,7000,7000);
/*!40000 ALTER TABLE `invoice_details` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `invoices`
--

DROP TABLE IF EXISTS `invoices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoices` (
  `invoice_id` varchar(20) NOT NULL,
  `store_id` varchar(10) NOT NULL,
  `staff_name` varchar(50) DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `total_origin` int NOT NULL,
  `voucher_code` varchar(50) DEFAULT NULL,
  `discount_amount` int DEFAULT '0',
  `final_total` int NOT NULL,
  PRIMARY KEY (`invoice_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoices`
--

LOCK TABLES `invoices` WRITE;
/*!40000 ALTER TABLE `invoices` DISABLE KEYS */;
INSERT INTO `invoices` VALUES ('HD260411222313','S01','Quản Trị Viên (Admin)','2026-04-11 22:23:13',3270000,'GIAM10',50000,3220000),('HD260411224132','S02','Quản lý Chi nhánh','2026-04-11 22:41:32',390000,NULL,0,390000),('HD260411225153','S01','Quản Trị Viên (Admin)','2026-04-11 22:51:53',500000,NULL,0,500000),('HD260413222224','S01','Quản Trị Viên (Admin)','2026-04-13 22:22:24',84000,'GIAM10',8400,75600),('HD260415213734','S01','Quản Trị Viên (Admin)','2026-04-15 21:37:34',32000,'SUPERSALE',16000,16000);
/*!40000 ALTER TABLE `invoices` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_details`
--

DROP TABLE IF EXISTS `order_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_details` (
  `id` int NOT NULL AUTO_INCREMENT,
  `order_id` int DEFAULT NULL,
  `product_id` varchar(10) DEFAULT NULL,
  `quantity` int DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  `specs_note` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `order_id` (`order_id`),
  CONSTRAINT `order_details_ibfk_1` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_details`
--

LOCK TABLES `order_details` WRITE;
/*!40000 ALTER TABLE `order_details` DISABLE KEYS */;
/*!40000 ALTER TABLE `order_details` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `store_id` varchar(10) DEFAULT NULL,
  `cashier_username` varchar(50) DEFAULT NULL,
  `total_amount` decimal(10,2) DEFAULT NULL,
  `discount_amount` decimal(10,2) DEFAULT NULL,
  `final_amount` decimal(10,2) DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `status` varchar(20) DEFAULT 'PENDING',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` varchar(10) NOT NULL,
  `name` varchar(100) NOT NULL,
  `type` varchar(20) DEFAULT NULL,
  `base_price` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES ('D01','Trà Sữa Matcha','Nước Uống',30000.00),('D02','Trà Đào Cam Sả','Nước Uống',35000.00),('D03','Sữa Tươi Trân Châu','Nước Uống',25000.00),('D04','Trà Sữa Truyền Thống','Nước Uống',30000.00),('D05','Trà Sữa Thái Xanh','Nước Uống',35000.00),('D06','Trà Sữa Oolong Nướng','Nước Uống',40000.00),('D07','Trà Vải Nhiệt Đới','Nước Uống',35000.00),('D08','Trà Đen Macchiato','Nước Uống',45000.00),('D09','Matcha Đá Xay','Nước Uống',50000.00),('D10','Sữa Tươi Trân Châu Đường Đen','Nước Uống',45000.00),('D11','Cà Phê Sữa Đá','Nước Uống',25000.00),('D12','Bạc Xỉu','Nước Uống',29000.00),('D13','Trà Dâu Tằm','Nước Uống',38000.00),('D14','Trà Sữa Socola','Nước Uống',35000.00),('T01','Trân Châu Đen','Topping',7000.00),('T02','Trân Châu Trắng','Topping',7000.00),('T03','Thạch Trái Cây','Topping',5000.00),('T04','Thạch Phô Mai','Topping',10000.00),('T05','Kem Macchiato','Topping',15000.00),('T06','Pudding Trứng','Topping',8000.00),('T07','Khúc Bạch','Topping',12000.00),('T08','Hạt Sen Bùi','Topping',10000.00);
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `stores`
--

DROP TABLE IF EXISTS `stores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stores` (
  `id` varchar(10) NOT NULL,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stores`
--

LOCK TABLES `stores` WRITE;
/*!40000 ALTER TABLE `stores` DISABLE KEYS */;
INSERT INTO `stores` VALUES ('S01','Chi Nhánh Trung Tâm'),('S02','CN02'),('S03','Chi nhánh Bà Điểm'),('S04','Chi Nhánh Mỹ Tho'),('S05','Chi Nhánh Nghệ An');
/*!40000 ALTER TABLE `stores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `support_reports`
--

DROP TABLE IF EXISTS `support_reports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `support_reports` (
  `id` int NOT NULL AUTO_INCREMENT,
  `store_id` varchar(10) DEFAULT NULL,
  `report_type` varchar(100) DEFAULT NULL,
  `message` text,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `is_resolved` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `support_reports`
--

LOCK TABLES `support_reports` WRITE;
/*!40000 ALTER TABLE `support_reports` DISABLE KEYS */;
INSERT INTO `support_reports` VALUES (1,'S01','Phản ánh của khách hàng','chê','2026-04-11 22:52:05',1),(2,'S02','Phản ánh của khách hàng','như cc','2026-04-13 14:00:40',0);
/*!40000 ALTER TABLE `support_reports` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vouchers`
--

DROP TABLE IF EXISTS `vouchers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vouchers` (
  `voucher_code` varchar(50) NOT NULL,
  `discount_percent` decimal(5,2) NOT NULL,
  `max_discount_amount` int NOT NULL,
  `expiry_date` date NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`voucher_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vouchers`
--

LOCK TABLES `vouchers` WRITE;
/*!40000 ALTER TABLE `vouchers` DISABLE KEYS */;
INSERT INTO `vouchers` VALUES ('GIAM10',10.00,50000,'2026-05-09','2026-04-11 15:22:35'),('SUPERSALE',50.00,20000,'2026-04-15','2026-04-13 15:28:25');
/*!40000 ALTER TABLE `vouchers` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-05-29 11:27:53
