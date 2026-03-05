# 🪷 SareeGrace — Indian Saree E-Commerce Platform

## Complete Project Plan & Architecture Document

---

## 📋 Table of Contents

1. [Project Overview](#1-project-overview)
2. [Tech Stack](#2-tech-stack)
3. [Feature Breakdown](#3-feature-breakdown)
4. [High-Level Architecture](#4-high-level-architecture)
5. [Frontend Architecture](#5-frontend-architecture)
6. [Backend Architecture](#6-backend-architecture)
7. [Database Design](#7-database-design)
8. [API Endpoints Design](#8-api-endpoints-design)
9. [Authentication Flow](#9-authentication-flow)
10. [Payment Gateway Flow](#10-payment-gateway-flow)
11. [Smart Business Logic](#11-smart-business-logic)
12. [Folder Structure](#12-folder-structure)
13. [Deployment Strategy](#13-deployment-strategy)
14. [Future Enhancement Roadmap](#14-future-enhancement-roadmap)

---

## 1. Project Overview

**SareeGrace** is a premium Indian saree e-commerce platform designed to emotionally connect with Indian women and girls. The platform showcases the rich diversity of Indian sarees — from luxurious Banarasi silk to everyday cotton, bridal collections to party wear — in a visually stunning, modern shopping experience.

### Vision
> "Every saree tells a story — we make discovering that story effortless and delightful."

### Key Differentiators
- **Emotionally resonant UI** with warm Indian color palettes (deep reds, golds, magentas, teals)
- **Smart stock management** with auto-discount on aging inventory
- **High-quality imagery** with zoom, 360° view, and drape visualization
- **Mobile-first responsive design** — 80%+ of Indian e-commerce traffic is mobile
- **Lightning-fast performance** with lazy loading and CDN-delivered assets

---

## 2. Tech Stack

| Layer | Technology | Justification |
|-------|-----------|---------------|
| **Frontend** | React.js 18+ (Vite) | Fast build, component-driven, massive ecosystem |
| **UI Library** | Tailwind CSS + Headless UI | Utility-first, highly customizable, premium feel |
| **State Management** | Redux Toolkit + RTK Query | Predictable state, built-in API caching |
| **Backend** | ASP.NET Core 8 Web API (C#) | Enterprise-grade, high-performance, secure |
| **ORM** | Entity Framework Core 8 | Code-first migrations, LINQ support |
| **Database** | SQL Server 2022 (Local via SSMS) → Azure SQL (production) | Start local, migrate to cloud when ready |
| **Authentication** | JWT + Refresh Tokens | Stateless, scalable, industry standard |
| **Payment** | Razorpay (primary) + Stripe (international) | Indian + global coverage |
| **Image Storage** | Local file system (dev) → Azure Blob Storage (production) | Start simple, scale to cloud |
| **CDN** | Azure CDN / CloudFront (production phase) | Fast global image delivery |
| **Email** | SendGrid / SMTP | Transactional emails (order confirm, OTP) |
| **Caching** | Redis | Session cache, product catalog cache |
| **Search** | Elasticsearch (future) / SQL Full-Text | Fast product search |
| **Logging** | Serilog + Seq | Structured logging |
| **CI/CD** | GitHub Actions / Azure DevOps | Automated build, test, deploy |
| **Hosting** | Localhost/IIS (dev) → Azure App Service (production) | Local first, cloud later |

---

## 3. Feature Breakdown

### 3.1 Customer-Facing Features

#### 🏠 Homepage
- Hero banner carousel with seasonal/festive collections
- Trending sarees section
- Category quick-access cards (Silk, Banarasi, Bridal, etc.)
- "Special Offers" section (auto-discounted aging inventory)
- New arrivals
- Testimonials & reviews
- Instagram feed integration
- Newsletter subscription

#### 👗 Saree Catalog
- **Category browsing**: Silk, Banarasi, Cotton, Chiffon, Georgette, Linen, Bridal, Designer, Party Wear, Casual, Festive
- **Advanced filters**: Price range, fabric type, color, pattern (floral, geometric, zari, etc.), occasion, brand, discount percentage
- **Sort**: Price (low-high, high-low), newest, popularity, rating, discount
- **Product cards**: Image, name, price, original price (if discounted), rating, "Special Offer" badge
- **Pagination / Infinite scroll**

#### 🔍 Product Detail Page
- High-resolution image gallery (5-8 images per saree)
- Pinch-to-zoom on mobile, hover-zoom on desktop
- Fabric, color, length, blouse piece details
- Size/length options
- Wash care instructions
- Delivery estimate (pincode-based)
- Customer reviews & ratings
- "Complete the Look" suggestions (blouse, jewelry, petticoat)
- Social sharing buttons
- Add to cart / Add to wishlist

#### 🛒 Shopping Cart
- Item list with quantity adjustment
- Price breakdown (MRP, discount, delivery, tax)
- Coupon code application
- "You might also like" recommendations
- Save for later

#### 💳 Checkout
- Address selection/addition
- Delivery method selection
- Payment method selection (UPI, Card, Net Banking, Wallet, COD)
- Order summary
- Secure payment processing
- Order confirmation page + email

#### 👤 User Account
- Registration (email/phone + OTP)
- Login (email/password + social login)
- Profile management (name, phone, photo)
- Address book (multiple addresses)
- Order history with status tracking
- Wishlist management
- Review management
- Notification preferences

#### 📦 Order Tracking
- Real-time order status (Placed → Confirmed → Shipped → Delivered)
- Tracking number with courier link
- Estimated delivery date
- Invoice download (PDF)

### 3.2 Admin Panel Features

#### 📊 Dashboard
- Sales overview (daily, weekly, monthly)
- Revenue charts
- Top-selling sarees
- Low-stock alerts
- Recent orders
- Customer growth metrics

#### 👗 Product Management
- Add new saree with full details + multiple images
- Edit existing products
- Bulk upload via CSV
- Stock count management
- Category management (CRUD)
- Fabric type management
- Color/pattern tag management

#### 📦 Order Management
- View all orders with filters (status, date, customer)
- Update order status
- Process refunds
- Generate invoices

#### 💰 Discount & Offers
- Create coupon codes (percentage / fixed amount)
- Set validity period
- View auto-discount items (>2 years old)
- Override auto-discount settings
- Flash sale management

#### 👥 Customer Management
- View all customers
- Customer order history
- Block/unblock accounts

#### 📈 Reports
- Sales reports
- Inventory reports
- Customer demographics
- Revenue analytics

---

## 4. High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  React SPA   │  │  Admin SPA   │  │  Mobile (PWA/Future) │  │
│  │  (Customer)  │  │  (Admin)     │  │                      │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────┬───────────┘  │
│         │                 │                      │              │
└─────────┼─────────────────┼──────────────────────┼──────────────┘
          │                 │                      │
          ▼                 ▼                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                     API GATEWAY / REVERSE PROXY                 │
│                        (Nginx / Azure API Mgmt)                 │
└─────────────────────────┬───────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│                   ASP.NET CORE WEB API                          │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────────┐  │
│  │ Auth      │ │ Product   │ │ Order     │ │ Payment       │  │
│  │ Controller│ │ Controller│ │ Controller│ │ Controller    │  │
│  └─────┬─────┘ └─────┬─────┘ └─────┬─────┘ └──────┬────────┘  │
│        │              │              │              │           │
│  ┌─────┴──────────────┴──────────────┴──────────────┴────────┐  │
│  │                  SERVICE LAYER                             │  │
│  │  AuthService │ ProductService │ OrderService │ PaymentSvc  │  │
│  └─────┬──────────────┬──────────────┬──────────────┬────────┘  │
│        │              │              │              │           │
│  ┌─────┴──────────────┴──────────────┴──────────────┴────────┐  │
│  │                REPOSITORY LAYER (EF Core)                  │  │
│  └───────────────────────┬───────────────────────────────────┘  │
└──────────────────────────┼──────────────────────────────────────┘
                           │
          ┌────────────────┼────────────────┐
          ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  SQL Server  │  │    Redis     │  │ Azure Blob   │
│  (Data)      │  │  (Cache)     │  │ (Images)     │
└──────────────┘  └──────────────┘  └──────────────┘
```

### Architecture Pattern: **Clean Architecture (Onion Architecture)**

```
┌────────────────────────────────────────────┐
│            Presentation Layer              │  ← Controllers, DTOs, Filters
│  ┌──────────────────────────────────────┐  │
│  │        Application Layer             │  │  ← Services, Interfaces, Commands
│  │  ┌────────────────────────────────┐  │  │
│  │  │       Domain Layer             │  │  │  ← Entities, Value Objects, Rules
│  │  │  ┌──────────────────────────┐  │  │  │
│  │  │  │   Infrastructure Layer   │  │  │  │  ← EF Core, External APIs
│  │  │  └──────────────────────────┘  │  │  │
│  │  └────────────────────────────────┘  │  │
│  └──────────────────────────────────────┘  │
└────────────────────────────────────────────┘
```

---

## 5. Frontend Architecture

### 5.1 Routing Structure

| Route | Component | Auth Required |
|-------|-----------|:---:|
| `/` | HomePage | No |
| `/sarees` | CatalogPage | No |
| `/sarees/:category` | CategoryPage | No |
| `/saree/:id` | ProductDetailPage | No |
| `/cart` | CartPage | No |
| `/checkout` | CheckoutPage | Yes |
| `/login` | LoginPage | No |
| `/register` | RegisterPage | No |
| `/account` | AccountDashboard | Yes |
| `/account/orders` | OrderHistoryPage | Yes |
| `/account/orders/:id` | OrderDetailPage | Yes |
| `/account/wishlist` | WishlistPage | Yes |
| `/account/addresses` | AddressPage | Yes |
| `/account/profile` | ProfilePage | Yes |
| `/search?q=` | SearchResultsPage | No |
| `/offers` | SpecialOffersPage | No |
| `/admin/*` | Admin Panel (separate SPA) | Yes (Admin) |

### 5.2 State Management Structure

```
Redux Store
├── auth/          → user, token, isAuthenticated
├── products/      → catalog, filters, search results (RTK Query)
├── cart/          → items, totals, coupon
├── wishlist/      → wishlist items
├── orders/        → order history, current order
├── ui/            → modals, toasts, loading states
└── admin/         → admin-specific state
```

### 5.3 Design System

**Color Palette:**
- Primary: Deep Magenta `#C2185B` / Rich Crimson `#D32F2F`
- Secondary: Royal Gold `#FFB300` / Antique Gold `#C89B3C`
- Accent: Teal `#00897B` / Deep Purple `#7B1FA2`
- Ivory Background: `#FFF8F0`
- Dark Text: `#2D1B14`
- Light Text: `#6D5D54`

**Typography:**
- Headings: Playfair Display (elegant serif)
- Body: Poppins (clean modern sans-serif)
- Hindi/Regional: Noto Sans Devanagari

**UI Components:**
- Floating "Add to Cart" button
- Smooth page transitions (Framer Motion)
- Skeleton loading states
- Toast notifications
- Modal-based quick view
- Sticky header with search
- Bottom navigation on mobile

---

## 6. Backend Architecture

### 6.1 Project Structure (Clean Architecture)

```
SareeGrace.sln
│
├── src/
│   ├── SareeGrace.Domain/              ← Domain Entities & Interfaces
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   │
│   ├── SareeGrace.Application/         ← Business Logic & DTOs
│   │   ├── DTOs/
│   │   ├── Services/
│   │   ├── Interfaces/
│   │   ├── Validators/
│   │   └── Mappings/
│   │
│   ├── SareeGrace.Infrastructure/      ← Data Access & External Services
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/         ← Fluent API configs
│   │   │   ├── Migrations/
│   │   │   └── Repositories/
│   │   ├── Services/
│   │   │   ├── PaymentService.cs
│   │   │   ├── EmailService.cs
│   │   │   ├── BlobStorageService.cs
│   │   │   └── CacheService.cs
│   │   └── Identity/
│   │
│   └── SareeGrace.API/                 ← Web API Layer
│       ├── Controllers/
│       ├── Middleware/
│       ├── Filters/
│       └── Program.cs
│
├── tests/
│   ├── SareeGrace.UnitTests/
│   ├── SareeGrace.IntegrationTests/
│   └── SareeGrace.API.Tests/
│
└── docs/
```

### 6.2 Middleware Pipeline

```
Request
  │
  ▼
[Exception Handling Middleware]
  │
  ▼
[Request Logging (Serilog)]
  │
  ▼
[CORS Policy]
  │
  ▼
[Rate Limiting]
  │
  ▼
[Authentication (JWT Bearer)]
  │
  ▼
[Authorization]
  │
  ▼
[Response Caching]
  │
  ▼
[Controller Action]
  │
  ▼
Response
```

---

## 7. Database Design

### 7.1 Entity-Relationship Overview

#### Core Tables

**Users**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | UNIQUEIDENTIFIER | PK, DEFAULT NEWSEQUENTIALID() |
| Email | NVARCHAR(256) | UNIQUE, NOT NULL |
| PasswordHash | NVARCHAR(MAX) | NOT NULL |
| FirstName | NVARCHAR(100) | NOT NULL |
| LastName | NVARCHAR(100) | |
| Phone | NVARCHAR(15) | |
| AvatarUrl | NVARCHAR(500) | |
| Role | NVARCHAR(20) | DEFAULT 'Customer' |
| IsActive | BIT | DEFAULT 1 |
| EmailVerified | BIT | DEFAULT 0 |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2 | |

**Categories**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| Name | NVARCHAR(100) | UNIQUE, NOT NULL |
| Slug | NVARCHAR(100) | UNIQUE, NOT NULL |
| Description | NVARCHAR(500) | |
| ImageUrl | NVARCHAR(500) | |
| ParentCategoryId | INT | FK → Categories(Id), NULLABLE |
| DisplayOrder | INT | DEFAULT 0 |
| IsActive | BIT | DEFAULT 1 |

**Products (Sarees)**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | UNIQUEIDENTIFIER | PK |
| Name | NVARCHAR(300) | NOT NULL |
| Slug | NVARCHAR(300) | UNIQUE, NOT NULL |
| Description | NVARCHAR(MAX) | |
| ShortDescription | NVARCHAR(500) | |
| SKU | NVARCHAR(50) | UNIQUE, NOT NULL |
| BasePrice | DECIMAL(10,2) | NOT NULL |
| DiscountPercent | DECIMAL(5,2) | DEFAULT 0 |
| SellingPrice | DECIMAL(10,2) | COMPUTED |
| FabricType | NVARCHAR(50) | NOT NULL |
| Color | NVARCHAR(50) | NOT NULL |
| Pattern | NVARCHAR(50) | |
| Occasion | NVARCHAR(50) | |
| Length | DECIMAL(5,2) | DEFAULT 5.5 (meters) |
| Width | DECIMAL(5,2) | DEFAULT 1.1 (meters) |
| HasBlousePiece | BIT | DEFAULT 0 |
| BlouseLength | DECIMAL(5,2) | NULLABLE |
| WashCare | NVARCHAR(300) | |
| Weight | DECIMAL(5,2) | grams |
| StockCount | INT | NOT NULL, DEFAULT 0 |
| CategoryId | INT | FK → Categories(Id) |
| IsActive | BIT | DEFAULT 1 |
| IsFeatured | BIT | DEFAULT 0 |
| AverageRating | DECIMAL(3,2) | DEFAULT 0 |
| ReviewCount | INT | DEFAULT 0 |
| DateAdded | DATETIME2 | DEFAULT GETUTCDATE() |
| ManufactureDate | DATETIME2 | NOT NULL |
| LastStockUpdate | DATETIME2 | |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2 | |

**ProductImages**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| ProductId | UNIQUEIDENTIFIER | FK → Products(Id) |
| ImageUrl | NVARCHAR(500) | NOT NULL |
| ThumbnailUrl | NVARCHAR(500) | |
| AltText | NVARCHAR(200) | |
| DisplayOrder | INT | DEFAULT 0 |
| IsPrimary | BIT | DEFAULT 0 |

**ProductTags**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| ProductId | UNIQUEIDENTIFIER | FK → Products(Id) |
| Tag | NVARCHAR(50) | NOT NULL |

**Addresses**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| UserId | UNIQUEIDENTIFIER | FK → Users(Id) |
| FullName | NVARCHAR(100) | NOT NULL |
| Phone | NVARCHAR(15) | NOT NULL |
| AddressLine1 | NVARCHAR(200) | NOT NULL |
| AddressLine2 | NVARCHAR(200) | |
| City | NVARCHAR(100) | NOT NULL |
| State | NVARCHAR(100) | NOT NULL |
| Pincode | NVARCHAR(10) | NOT NULL |
| Country | NVARCHAR(50) | DEFAULT 'India' |
| IsDefault | BIT | DEFAULT 0 |
| AddressType | NVARCHAR(20) | 'Home' / 'Office' |

**Orders**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | UNIQUEIDENTIFIER | PK |
| OrderNumber | NVARCHAR(20) | UNIQUE, AUTO-GENERATED |
| UserId | UNIQUEIDENTIFIER | FK → Users(Id) |
| ShippingAddressId | INT | FK → Addresses(Id) |
| OrderStatus | NVARCHAR(30) | NOT NULL |
| SubTotal | DECIMAL(10,2) | NOT NULL |
| DiscountAmount | DECIMAL(10,2) | DEFAULT 0 |
| TaxAmount | DECIMAL(10,2) | DEFAULT 0 |
| ShippingCharge | DECIMAL(10,2) | DEFAULT 0 |
| TotalAmount | DECIMAL(10,2) | NOT NULL |
| CouponCode | NVARCHAR(50) | |
| PaymentMethod | NVARCHAR(30) | |
| PaymentId | NVARCHAR(100) | |
| PaymentStatus | NVARCHAR(30) | |
| TrackingNumber | NVARCHAR(100) | |
| CourierName | NVARCHAR(50) | |
| EstimatedDelivery | DATETIME2 | |
| DeliveredAt | DATETIME2 | |
| Notes | NVARCHAR(500) | |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2 | |

**OrderItems**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| OrderId | UNIQUEIDENTIFIER | FK → Orders(Id) |
| ProductId | UNIQUEIDENTIFIER | FK → Products(Id) |
| ProductName | NVARCHAR(300) | snapshot at order time |
| Quantity | INT | NOT NULL |
| UnitPrice | DECIMAL(10,2) | NOT NULL |
| DiscountPercent | DECIMAL(5,2) | DEFAULT 0 |
| TotalPrice | DECIMAL(10,2) | NOT NULL |
| ImageUrl | NVARCHAR(500) | snapshot at order time |

**Reviews**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| ProductId | UNIQUEIDENTIFIER | FK → Products(Id) |
| UserId | UNIQUEIDENTIFIER | FK → Users(Id) |
| Rating | INT | CHECK (1-5) |
| Title | NVARCHAR(200) | |
| Comment | NVARCHAR(MAX) | |
| IsVerifiedPurchase | BIT | DEFAULT 0 |
| IsApproved | BIT | DEFAULT 0 |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |

**Wishlists**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| UserId | UNIQUEIDENTIFIER | FK → Users(Id) |
| ProductId | UNIQUEIDENTIFIER | FK → Products(Id) |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |
| | | UNIQUE(UserId, ProductId) |

**Coupons**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| Code | NVARCHAR(50) | UNIQUE, NOT NULL |
| Description | NVARCHAR(200) | |
| DiscountType | NVARCHAR(20) | 'Percentage' / 'Fixed' |
| DiscountValue | DECIMAL(10,2) | NOT NULL |
| MinOrderAmount | DECIMAL(10,2) | DEFAULT 0 |
| MaxDiscountAmount | DECIMAL(10,2) | |
| UsageLimit | INT | |
| UsedCount | INT | DEFAULT 0 |
| ValidFrom | DATETIME2 | NOT NULL |
| ValidTo | DATETIME2 | NOT NULL |
| IsActive | BIT | DEFAULT 1 |

**CartItems**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| UserId | UNIQUEIDENTIFIER | FK → Users(Id) |
| ProductId | UNIQUEIDENTIFIER | FK → Products(Id) |
| Quantity | INT | DEFAULT 1 |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |
| | | UNIQUE(UserId, ProductId) |

**RefreshTokens**
| Column | Type | Constraints |
|--------|------|-------------|
| Id | INT | PK, IDENTITY |
| UserId | UNIQUEIDENTIFIER | FK → Users(Id) |
| Token | NVARCHAR(500) | NOT NULL |
| ExpiresAt | DATETIME2 | NOT NULL |
| CreatedAt | DATETIME2 | DEFAULT GETUTCDATE() |
| RevokedAt | DATETIME2 | |
| ReplacedByToken | NVARCHAR(500) | |

### 7.2 Key Indexes

```sql
-- Performance indexes
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_FabricType ON Products(FabricType);
CREATE INDEX IX_Products_Color ON Products(Color);
CREATE INDEX IX_Products_BasePrice ON Products(BasePrice);
CREATE INDEX IX_Products_ManufactureDate ON Products(ManufactureDate);
CREATE INDEX IX_Products_IsActive_IsFeatured ON Products(IsActive, IsFeatured);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Wishlists_UserId ON Wishlists(UserId);
CREATE INDEX IX_CartItems_UserId ON CartItems(UserId);

-- Full-text search
CREATE FULLTEXT INDEX ON Products(Name, Description, ShortDescription)
    KEY INDEX PK_Products ON ProductSearchCatalog;
```

---

## 8. API Endpoints Design

### 8.1 Authentication APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login with credentials | No |
| POST | `/api/auth/refresh-token` | Refresh access token | No |
| POST | `/api/auth/logout` | Revoke refresh token | Yes |
| POST | `/api/auth/forgot-password` | Send password reset email | No |
| POST | `/api/auth/reset-password` | Reset password with token | No |
| POST | `/api/auth/verify-email` | Verify email address | No |
| GET | `/api/auth/me` | Get current user info | Yes |

### 8.2 Product APIs (Public)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/products` | Browse products (paginated, filtered) | No |
| GET | `/api/products/{id}` | Get product details | No |
| GET | `/api/products/slug/{slug}` | Get product by slug | No |
| GET | `/api/products/featured` | Get featured products | No |
| GET | `/api/products/special-offers` | Get auto-discounted products | No |
| GET | `/api/products/new-arrivals` | Get latest products | No |
| GET | `/api/products/{id}/reviews` | Get product reviews | No |
| GET | `/api/products/search?q=&filters=` | Search with filters | No |
| GET | `/api/categories` | Get all categories | No |
| GET | `/api/categories/{slug}/products` | Get products by category | No |

### 8.3 Customer APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/cart` | Get user's cart | Yes |
| POST | `/api/cart` | Add item to cart | Yes |
| PUT | `/api/cart/{itemId}` | Update cart item quantity | Yes |
| DELETE | `/api/cart/{itemId}` | Remove cart item | Yes |
| DELETE | `/api/cart` | Clear cart | Yes |
| GET | `/api/wishlist` | Get user's wishlist | Yes |
| POST | `/api/wishlist/{productId}` | Add to wishlist | Yes |
| DELETE | `/api/wishlist/{productId}` | Remove from wishlist | Yes |
| GET | `/api/addresses` | Get user's addresses | Yes |
| POST | `/api/addresses` | Add new address | Yes |
| PUT | `/api/addresses/{id}` | Update address | Yes |
| DELETE | `/api/addresses/{id}` | Delete address | Yes |
| POST | `/api/orders` | Place new order | Yes |
| GET | `/api/orders` | Get user's order history | Yes |
| GET | `/api/orders/{id}` | Get order details | Yes |
| GET | `/api/orders/{id}/invoice` | Download invoice PDF | Yes |
| POST | `/api/orders/{id}/cancel` | Cancel order | Yes |
| POST | `/api/reviews` | Submit product review | Yes |
| PUT | `/api/profile` | Update user profile | Yes |
| PUT | `/api/profile/change-password` | Change password | Yes |
| POST | `/api/coupons/validate` | Validate coupon code | Yes |

### 8.4 Payment APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/payments/create-order` | Create Razorpay order | Yes |
| POST | `/api/payments/verify` | Verify payment signature | Yes |
| POST | `/api/payments/webhook` | Razorpay webhook handler | No* |

*Webhook verified by Razorpay signature

### 8.5 Admin APIs

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/admin/dashboard` | Dashboard statistics | Admin |
| GET | `/api/admin/products` | List all products (paginated) | Admin |
| POST | `/api/admin/products` | Create new product | Admin |
| PUT | `/api/admin/products/{id}` | Update product | Admin |
| DELETE | `/api/admin/products/{id}` | Soft-delete product | Admin |
| PUT | `/api/admin/products/{id}/stock` | Update stock count | Admin |
| POST | `/api/admin/products/bulk-upload` | CSV bulk upload | Admin |
| POST | `/api/admin/products/{id}/images` | Upload product images | Admin |
| DELETE | `/api/admin/products/images/{imageId}` | Delete product image | Admin |
| GET | `/api/admin/categories` | List categories | Admin |
| POST | `/api/admin/categories` | Create category | Admin |
| PUT | `/api/admin/categories/{id}` | Update category | Admin |
| DELETE | `/api/admin/categories/{id}` | Delete category | Admin |
| GET | `/api/admin/orders` | List all orders | Admin |
| PUT | `/api/admin/orders/{id}/status` | Update order status | Admin |
| POST | `/api/admin/orders/{id}/refund` | Process refund | Admin |
| GET | `/api/admin/customers` | List customers | Admin |
| GET | `/api/admin/customers/{id}` | Customer details | Admin |
| GET | `/api/admin/coupons` | List coupons | Admin |
| POST | `/api/admin/coupons` | Create coupon | Admin |
| PUT | `/api/admin/coupons/{id}` | Update coupon | Admin |
| DELETE | `/api/admin/coupons/{id}` | Delete coupon | Admin |
| GET | `/api/admin/reviews` | List pending reviews | Admin |
| PUT | `/api/admin/reviews/{id}/approve` | Approve review | Admin |
| DELETE | `/api/admin/reviews/{id}` | Delete review | Admin |
| GET | `/api/admin/reports/sales` | Sales report | Admin |
| GET | `/api/admin/reports/inventory` | Inventory report | Admin |

### 8.6 Standard API Response Format

```json
// Success
{
  "success": true,
  "data": { ... },
  "message": "Products fetched successfully",
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 450,
    "totalPages": 23
  }
}

// Error
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    { "field": "email", "message": "Email is already registered" }
  ]
}
```

---

## 9. Authentication Flow

```
┌──────────┐     1. Register/Login       ┌──────────────┐
│  Client   │ ─────────────────────────▶  │  API Server  │
│  (React)  │                             │  (ASP.NET)   │
│           │  2. JWT Access Token +      │              │
│           │     Refresh Token (cookie)  │              │
│           │ ◀─────────────────────────  │              │
│           │                             │              │
│           │  3. API Call with           │              │
│           │     Bearer Token            │              │
│           │ ─────────────────────────▶  │              │
│           │                             │  ┌────────┐  │
│           │  4. Protected Data          │  │ JWT    │  │
│           │ ◀─────────────────────────  │  │Validate│  │
│           │                             │  └────────┘  │
│           │  5. Token Expired (401)     │              │
│           │ ◀─────────────────────────  │              │
│           │                             │              │
│           │  6. Refresh Token Request   │              │
│           │ ─────────────────────────▶  │              │
│           │                             │              │
│           │  7. New Access Token        │              │
│           │ ◀─────────────────────────  │              │
└──────────┘                              └──────────────┘
```

### Token Details:
- **Access Token**: JWT, 15-minute expiry, contains userId, email, role
- **Refresh Token**: Opaque token, 7-day expiry, stored in HttpOnly secure cookie + DB
- **Token Rotation**: Each refresh issues a new refresh token and revokes the old one
- **Password Hashing**: BCrypt with salt rounds = 12

### Role-Based Access:
- `Customer` — Default role, shopping features
- `Admin` — Full admin panel access
- `SuperAdmin` — System configuration, user management

---

## 10. Payment Gateway Flow

### Razorpay Integration Flow

```
┌──────────┐          ┌──────────────┐          ┌──────────────┐
│  Client   │          │  API Server  │          │   Razorpay   │
│  (React)  │          │  (ASP.NET)   │          │   Server     │
└─────┬─────┘          └──────┬───────┘          └──────┬───────┘
      │                       │                         │
      │  1. Click "Pay Now"   │                         │
      │ ─────────────────────▶│                         │
      │                       │  2. Create Order         │
      │                       │ ────────────────────────▶│
      │                       │                         │
      │                       │  3. Order ID + Key       │
      │                       │ ◀────────────────────────│
      │  4. Order details     │                         │
      │ ◀─────────────────────│                         │
      │                       │                         │
      │  5. Open Razorpay     │                         │
      │     Checkout Modal    │                         │
      │ ─────────────────────────────────────────────────▶
      │                       │                         │
      │  6. Payment Success   │                         │
      │     (razorpay_payment_id,                       │
      │      razorpay_order_id,                         │
      │      razorpay_signature)                        │
      │ ◀─────────────────────────────────────────────────
      │                       │                         │
      │  7. Send verification │                         │
      │     payload           │                         │
      │ ─────────────────────▶│                         │
      │                       │  8. Verify Signature     │
      │                       │  (HMAC SHA256)           │
      │                       │                         │
      │                       │  9. Update Order Status  │
      │                       │     Reduce Stock Count   │
      │                       │                         │
      │  10. Order Confirmed  │                         │
      │ ◀─────────────────────│                         │
      │                       │                         │
      │                       │  11. Webhook (backup)    │
      │                       │ ◀────────────────────────│
      │                       │                         │
```

### Payment Methods Supported:
- UPI (Google Pay, PhonePe, Paytm)
- Debit/Credit Cards (Visa, Mastercard, RuPay)
- Net Banking
- Wallets
- EMI
- Cash on Delivery (COD) — separate flow, no gateway

---

## 11. Smart Business Logic

### 11.1 Auto-Discount for Aging Inventory

```csharp
// Runs as a daily background job (Hangfire / .NET BackgroundService)
public class AgingInventoryService
{
    private const int AGING_THRESHOLD_YEARS = 2;
    private const decimal AUTO_DISCOUNT_PERCENT = 10m;

    public async Task ApplyAgingDiscounts()
    {
        var thresholdDate = DateTime.UtcNow.AddYears(-AGING_THRESHOLD_YEARS);

        var agingProducts = await _repository.GetProducts(p =>
            p.ManufactureDate <= thresholdDate &&
            p.StockCount > 0 &&
            p.IsActive &&
            p.DiscountPercent < AUTO_DISCOUNT_PERCENT);

        foreach (var product in agingProducts)
        {
            product.DiscountPercent = AUTO_DISCOUNT_PERCENT;
            product.Tags.Add("Special Offer");
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _repository.SaveChangesAsync();
    }
}
```

### 11.2 Stock Management Rules
- Stock decremented on successful payment (not on order placement)
- Stock restored if payment fails or order is cancelled within 24 hours
- Low-stock alert when count < 5
- Out-of-stock products shown with "Notify Me" option
- Stock reservation during checkout (10-minute hold)

### 11.3 Pricing Computation
```
Selling Price = Base Price - (Base Price × Discount Percent / 100)
```
- Selling Price is a **computed column** in SQL Server
- Both manual discounts and auto-discounts are stored in `DiscountPercent`
- Admin can override auto-discount

---

## 12. Folder Structure

### 12.1 Frontend (React)

```
sareegrace-client/
├── public/
│   ├── favicon.ico
│   ├── logo192.png
│   └── manifest.json
├── src/
│   ├── api/                        # RTK Query API slices
│   │   ├── authApi.ts
│   │   ├── productsApi.ts
│   │   ├── ordersApi.ts
│   │   ├── cartApi.ts
│   │   └── adminApi.ts
│   ├── app/                        # Redux store setup
│   │   ├── store.ts
│   │   └── hooks.ts
│   ├── assets/                     # Static assets
│   │   ├── images/
│   │   ├── icons/
│   │   └── fonts/
│   ├── components/                 # Reusable components
│   │   ├── common/
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Modal.tsx
│   │   │   ├── Loader.tsx
│   │   │   ├── Rating.tsx
│   │   │   ├── Pagination.tsx
│   │   │   ├── Toast.tsx
│   │   │   ├── Badge.tsx
│   │   │   └── ImageZoom.tsx
│   │   ├── layout/
│   │   │   ├── Header.tsx
│   │   │   ├── Footer.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   ├── MobileNav.tsx
│   │   │   └── Layout.tsx
│   │   ├── product/
│   │   │   ├── ProductCard.tsx
│   │   │   ├── ProductGrid.tsx
│   │   │   ├── ProductGallery.tsx
│   │   │   ├── ProductFilters.tsx
│   │   │   ├── QuickView.tsx
│   │   │   └── ReviewCard.tsx
│   │   ├── cart/
│   │   │   ├── CartItem.tsx
│   │   │   ├── CartSummary.tsx
│   │   │   └── CouponInput.tsx
│   │   ├── checkout/
│   │   │   ├── AddressSelector.tsx
│   │   │   ├── PaymentMethod.tsx
│   │   │   └── OrderSummary.tsx
│   │   └── home/
│   │       ├── HeroBanner.tsx
│   │       ├── CategoryCards.tsx
│   │       ├── TrendingSection.tsx
│   │       ├── SpecialOffers.tsx
│   │       └── Testimonials.tsx
│   ├── features/                   # Redux slices
│   │   ├── auth/
│   │   │   └── authSlice.ts
│   │   ├── cart/
│   │   │   └── cartSlice.ts
│   │   └── ui/
│   │       └── uiSlice.ts
│   ├── hooks/                      # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useCart.ts
│   │   ├── useDebounce.ts
│   │   └── useIntersection.ts
│   ├── pages/                      # Page components
│   │   ├── HomePage.tsx
│   │   ├── CatalogPage.tsx
│   │   ├── ProductDetailPage.tsx
│   │   ├── CartPage.tsx
│   │   ├── CheckoutPage.tsx
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── SearchResultsPage.tsx
│   │   ├── SpecialOffersPage.tsx
│   │   ├── account/
│   │   │   ├── AccountDashboard.tsx
│   │   │   ├── OrderHistoryPage.tsx
│   │   │   ├── OrderDetailPage.tsx
│   │   │   ├── WishlistPage.tsx
│   │   │   ├── AddressPage.tsx
│   │   │   └── ProfilePage.tsx
│   │   ├── admin/
│   │   │   ├── AdminLayout.tsx
│   │   │   ├── DashboardPage.tsx
│   │   │   ├── ProductsManagePage.tsx
│   │   │   ├── AddProductPage.tsx
│   │   │   ├── OrdersManagePage.tsx
│   │   │   ├── CustomersPage.tsx
│   │   │   ├── CouponsPage.tsx
│   │   │   ├── CategoriesPage.tsx
│   │   │   └── ReportsPage.tsx
│   │   └── NotFoundPage.tsx
│   ├── routes/                     # Route configuration
│   │   ├── AppRoutes.tsx
│   │   ├── ProtectedRoute.tsx
│   │   └── AdminRoute.tsx
│   ├── styles/                     # Global styles
│   │   ├── globals.css
│   │   └── tailwind.css
│   ├── types/                      # TypeScript types
│   │   ├── product.ts
│   │   ├── order.ts
│   │   ├── user.ts
│   │   └── api.ts
│   ├── utils/                      # Utility functions
│   │   ├── formatPrice.ts
│   │   ├── dateUtils.ts
│   │   ├── validators.ts
│   │   └── constants.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── vite-env.d.ts
├── .env
├── .env.example
├── index.html
├── package.json
├── postcss.config.js
├── tailwind.config.js
├── tsconfig.json
└── vite.config.ts
```

### 12.2 Backend (ASP.NET Core)

```
SareeGrace/
├── src/
│   ├── SareeGrace.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Product.cs
│   │   │   ├── ProductImage.cs
│   │   │   ├── Category.cs
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   ├── CartItem.cs
│   │   │   ├── Wishlist.cs
│   │   │   ├── Review.cs
│   │   │   ├── Address.cs
│   │   │   ├── Coupon.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Enums/
│   │   │   ├── OrderStatus.cs
│   │   │   ├── PaymentStatus.cs
│   │   │   ├── FabricType.cs
│   │   │   └── UserRole.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── IProductRepository.cs
│   │   │   ├── IOrderRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── SareeGrace.Domain.csproj
│   │
│   ├── SareeGrace.Application/
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   │   ├── LoginDto.cs
│   │   │   │   ├── RegisterDto.cs
│   │   │   │   └── TokenDto.cs
│   │   │   ├── Products/
│   │   │   │   ├── ProductDto.cs
│   │   │   │   ├── ProductDetailDto.cs
│   │   │   │   ├── CreateProductDto.cs
│   │   │   │   └── ProductFilterDto.cs
│   │   │   ├── Orders/
│   │   │   │   ├── OrderDto.cs
│   │   │   │   ├── CreateOrderDto.cs
│   │   │   │   └── OrderItemDto.cs
│   │   │   └── Common/
│   │   │       ├── ApiResponse.cs
│   │   │       └── PaginatedResult.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── IProductService.cs
│   │   │   ├── IOrderService.cs
│   │   │   ├── ICartService.cs
│   │   │   ├── IPaymentService.cs
│   │   │   ├── IEmailService.cs
│   │   │   └── IImageStorageService.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── ProductService.cs
│   │   │   ├── OrderService.cs
│   │   │   ├── CartService.cs
│   │   │   ├── WishlistService.cs
│   │   │   ├── CouponService.cs
│   │   │   ├── ReviewService.cs
│   │   │   └── AgingInventoryService.cs
│   │   ├── Validators/
│   │   │   ├── RegisterValidator.cs
│   │   │   ├── CreateProductValidator.cs
│   │   │   └── CreateOrderValidator.cs
│   │   ├── Mappings/
│   │   │   └── AutoMapperProfile.cs
│   │   └── SareeGrace.Application.csproj
│   │
│   ├── SareeGrace.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── ProductConfiguration.cs
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   └── ...
│   │   │   ├── Repositories/
│   │   │   │   ├── GenericRepository.cs
│   │   │   │   ├── ProductRepository.cs
│   │   │   │   ├── OrderRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Migrations/
│   │   │   └── Seed/
│   │   │       └── DataSeeder.cs
│   │   ├── Services/
│   │   │   ├── RazorpayPaymentService.cs
│   │   │   ├── SendGridEmailService.cs
│   │   │   ├── AzureBlobStorageService.cs
│   │   │   ├── RedisCacheService.cs
│   │   │   └── InvoiceGeneratorService.cs
│   │   └── SareeGrace.Infrastructure.csproj
│   │
│   └── SareeGrace.API/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── ProductsController.cs
│       │   ├── CartController.cs
│       │   ├── WishlistController.cs
│       │   ├── OrdersController.cs
│       │   ├── PaymentsController.cs
│       │   ├── AddressesController.cs
│       │   ├── ReviewsController.cs
│       │   ├── CategoriesController.cs
│       │   ├── ProfileController.cs
│       │   └── Admin/
│       │       ├── AdminDashboardController.cs
│       │       ├── AdminProductsController.cs
│       │       ├── AdminOrdersController.cs
│       │       ├── AdminCustomersController.cs
│       │       ├── AdminCouponsController.cs
│       │       └── AdminReportsController.cs
│       ├── Middleware/
│       │   ├── ExceptionMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── Filters/
│       │   └── ValidationFilter.cs
│       ├── BackgroundJobs/
│       │   └── AgingInventoryJob.cs
│       ├── Extensions/
│       │   ├── ServiceExtensions.cs
│       │   └── MiddlewareExtensions.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Program.cs
│       └── SareeGrace.API.csproj
│
├── tests/
│   ├── SareeGrace.UnitTests/
│   ├── SareeGrace.IntegrationTests/
│   └── SareeGrace.API.Tests/
│
├── docker-compose.yml
├── .dockerignore
├── .gitignore
├── README.md
└── SareeGrace.sln
```

---

## 13. Deployment Strategy

### 13.0 Database Strategy — Phased Approach

> **Phase A (Current — Development & Initial Launch):**
> Use **SQL Server (local instance)** managed via **SQL Server Management Studio (SSMS)**.
> All development, testing, and initial deployment will target a local/on-premise SQL Server.
> Connection string will point to `localhost` or a named SQL Server instance.
>
> **Phase B (Post-Launch — Cloud Migration):**
> Once the application is fully functional and deployed successfully on local SQL Server,
> we will migrate the database to **Azure SQL Database** for production scalability.
> EF Core migrations + connection string swap makes this a seamless transition.

```
📌 Database Migration Path:

  ┌───────────────────────────┐         ┌───────────────────────────┐
  │  PHASE A (NOW)            │         │  PHASE B (LATER)          │
  │                           │         │                           │
  │  SQL Server (Local)       │  ────▶  │  Azure SQL Database       │
  │  Managed via SSMS         │         │  Managed via Azure Portal │
  │  Connection: localhost    │         │  Connection: Azure conn   │
  │  Images: local wwwroot    │         │  Images: Azure Blob + CDN │
  │  Hosting: IIS / Kestrel   │         │  Hosting: Azure App Svc   │
  └───────────────────────────┘         └───────────────────────────┘

  What changes:  Only appsettings.json connection strings & storage config
  What stays:    All EF Core code, migrations, entities, business logic
```

### 13.1 Environment Setup

| Environment | Purpose | Database | URL |
|------------|---------|----------|-----|
| **Development** | Local dev with hot reload | SQL Server (Local/SSMS) | `localhost:5173` (FE) / `localhost:5000` (API) |
| **Staging** | QA and testing | SQL Server (Local/SSMS) | `staging.sareegrace.com` |
| **Production (Phase A)** | Initial live deployment | SQL Server (Local/SSMS) | `www.sareegrace.com` |
| **Production (Phase B)** | Cloud-scaled deployment | Azure SQL Database | `www.sareegrace.com` |

### 13.2 Phase A — Local Infrastructure (Current)

```
┌──────────────────────────────────────────────────────────────┐
│                     LOCAL DEVELOPMENT SETUP                   │
│                                                              │
│   ┌─────────────────┐        ┌──────────────────────────┐   │
│   │  React Dev       │        │  ASP.NET Core API         │   │
│   │  Server (Vite)   │ ──────▶│  (Kestrel / IIS Express)  │   │
│   │  localhost:5173  │        │  localhost:5000            │   │
│   └─────────────────┘        └────────────┬─────────────┘   │
│                                            │                 │
│                              ┌─────────────┴──────────────┐  │
│                              ▼                            ▼  │
│                    ┌──────────────┐           ┌───────────┐  │
│                    │ SQL Server   │           │ wwwroot/  │  │
│                    │ (Local SSMS) │           │ images/   │  │
│                    └──────────────┘           └───────────┘  │
└──────────────────────────────────────────────────────────────┘
```

### 13.3 Phase B — Azure Cloud Infrastructure (Future)

```
┌────────────────────────────────────────────────────────────┐
│                        CLOUDFLARE                          │
│                   (DNS + CDN + SSL + WAF)                  │
└───────────────────────────┬────────────────────────────────┘
                            │
              ┌─────────────┴─────────────┐
              ▼                           ▼
┌──────────────────────┐    ┌──────────────────────┐
│   Azure Static Web   │    │   Azure App Service  │
│   (React Frontend)   │    │   (ASP.NET API)      │
│                      │    │                      │
│   CDN-delivered      │    │   Auto-scaling       │
│   Global edge cache  │    │   Load balanced      │
└──────────────────────┘    └──────────┬───────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    ▼                  ▼                  ▼
          ┌──────────────┐   ┌──────────────┐   ┌──────────────┐
          │ Azure SQL DB │   │    Redis     │   │  Azure Blob  │
          │ (Database)   │   │  (Cache)     │   │  (Images)    │
          └──────────────┘   └──────────────┘   └──────────────┘
```

### 13.4 CI/CD Pipeline (GitHub Actions)

```yaml
# Phase A (Local): Manual build & deploy to local IIS / Kestrel
# Phase B (Azure): Automated pipeline

Trigger: Push to main / PR merge
  │
  ├── Build Frontend
  │   ├── Install dependencies
  │   ├── Run linting
  │   ├── Run unit tests
  │   ├── Build production bundle
  │   └── Deploy to Azure Static Web Apps (Phase B)
  │
  └── Build Backend
      ├── Restore NuGet packages
      ├── Build solution
      ├── Run unit tests
      ├── Run integration tests
      ├── Publish API
      └── Deploy to Azure App Service (Phase B)
```

### 13.5 Docker Support

```dockerfile
# Multi-stage build for API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Build and publish
FROM base AS final
EXPOSE 80 443
```

### 13.6 Monitoring & Observability
- **Logging**: Serilog → Seq / Azure Application Insights
- **Health Checks**: `/health` and `/health/ready` endpoints
- **Metrics**: Application Insights / Prometheus + Grafana
- **Alerts**: Email + Slack notification on error spikes
- **Uptime**: Azure Monitor / UptimeRobot

---

## 14. Future Enhancement Roadmap

### Phase 1 — MVP (Weeks 1-8) ✅
- [x] User authentication (register, login, JWT)
- [x] Product catalog with filters
- [x] Shopping cart & wishlist
- [x] Checkout with Razorpay
- [x] Order management
- [x] Basic admin panel
- [x] Smart aging discount system

### Phase 2 — Enhanced Experience (Weeks 9-12)
- [ ] Product reviews & ratings
- [ ] Email notifications (order confirmation, shipping)
- [ ] Invoice PDF generation
- [ ] Coupon/promo code system
- [ ] Advanced search with Elasticsearch
- [ ] Performance optimization (Redis caching)

### Phase 3 — Growth Features (Weeks 13-18)
- [ ] Social login (Google, Facebook)
- [ ] Multi-language support (Hindi, Tamil, Telugu, Bengali)
- [ ] Size recommendation AI
- [ ] "Try On" AR feature (virtual draping)
- [ ] WhatsApp order updates
- [ ] Push notifications (Firebase)
- [ ] Referral program
- [ ] Loyalty points system

### Phase 4 — Scale & Intelligence (Weeks 19-24)
- [ ] AI-powered product recommendations
- [ ] Personalized homepage
- [ ] A/B testing framework
- [ ] Advanced analytics dashboard
- [ ] Vendor/seller marketplace model
- [ ] Mobile app (React Native)
- [ ] Voice search in Indian languages
- [ ] Chatbot support (AI-powered)

### Phase 5 — Enterprise (6+ months)
- [ ] Multi-warehouse inventory
- [ ] ERP integration
- [ ] B2B wholesale portal
- [ ] International shipping
- [ ] Multi-currency support
- [ ] Custom saree designer tool
- [ ] Live video shopping events
- [ ] Subscription box (monthly curated sarees)

---

## Summary

| Aspect | Decision |
|--------|----------|
| **Architecture** | Clean Architecture (Onion) with CQRS-ready structure |
| **Frontend** | React + TypeScript + Vite + Tailwind CSS + Redux Toolkit |
| **Backend** | ASP.NET Core 8 Web API + EF Core 8 |
| **Database** | SQL Server Local (SSMS) → Azure SQL (Phase B) |
| **Auth** | JWT + Refresh Token rotation |
| **Payment** | Razorpay (primary) + Stripe (international) |
| **Image Storage** | Local wwwroot (Phase A) → Azure Blob + CDN (Phase B) |
| **Caching** | In-memory (Phase A) → Redis (Phase B) |
| **Background Jobs** | .NET BackgroundService / Hangfire |
| **Deployment** | Local SQL Server + IIS (Phase A) → Azure full stack (Phase B) |
| **CI/CD** | GitHub Actions |
| **Monitoring** | Serilog + Application Insights |

---

### Database Connection Strategy (Code Implementation)

```csharp
// appsettings.Development.json (Phase A — Local SQL Server via SSMS)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SareeGraceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "ImageStorage": {
    "Provider": "Local",      // Saves to wwwroot/images/
    "BasePath": "wwwroot/images/products"
  }
}

// appsettings.Production.json (Phase B — Azure SQL)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:sareegrace.database.windows.net,1433;Database=SareeGraceDB;User ID=admin;Password=***;Encrypt=True;"
  },
  "ImageStorage": {
    "Provider": "AzureBlob",
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=sareegraceimages;...",
    "ContainerName": "product-images"
  }
}
```

> **Key Principle:** The codebase uses interfaces (`IImageStorageService`, `IDbContext`) so switching
> from local SQL Server to Azure SQL requires **only a connection string change** — zero code changes.
> EF Core handles both identically.

---

*Document Version: 1.1*
*Updated: March 1, 2026*
*Project: SareeGrace — Indian Saree E-Commerce Platform*
*Database Strategy: SQL Server (SSMS) → Azure SQL (phased migration)*
