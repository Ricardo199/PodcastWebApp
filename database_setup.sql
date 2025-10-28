/*
 * Authors: Ricardo Burgos & [Partner Name]
 * Course: COMP306 - API Engineering
 * Lab: #3 - Podcast Web Application
 * 
 * SQL Server Database Setup - Structured Data Only
 * Note: Comments are stored in-memory (simulating DynamoDB)
 */

-- Create Database
CREATE DATABASE PodcastDB;
GO

USE PodcastDB;
GO

-- Note: All structured data tables (Users, Episodes, Podcasts, Subscriptions) 
-- are created automatically by EF Core migrations
-- This file only contains sample data

-- Episodes Table will be created by EF Core
-- Podcasts Table will be created by EF Core  
-- Subscriptions Table will be created by EF Core
-- User Tables will be created by ASP.NET Identity

-- Note: EF Core migrations will create all tables automatically
-- Run: dotnet ef database update