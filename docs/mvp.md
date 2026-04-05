# MVP document for the project

## Goal
The goal of the project is creating application for menaging work utilizing canban table.

## Problem
The project resolves problem with work management.

## Target user
The target user is the person who wants to manage one's work by dashboard with tasks.

## MVP
- adding tasks
- deleting tasks
- editing tasks
- registering
- log in
- viewing tasks


## MoSCoW
Must have:
- adding tasks
- deleting tasks
- editing tasks
- registering
- log in
- viewing tasks

Should have:
- labels

Could have:
- sorting
- filtering
- notification
- comments
- due dates
- drag-and-drop

Will not have:
- team-collaboration
- advanced-reporting
- mobile app


## User stories
- As a user I want to log in
- As a user I want to register
- As a user I want to add tasks
- As a user I want to remove tasks
- As a user I want to modify tasks
- As a user I want to view my tasks

## Functional requirements
- The system should allow to log in with email and password
- The system should allow to register user with email and password
- The system should allow to add task with title and description
- The system should allow to remove tasks by clicking on the remove button on task pop up
- the system should allow to modify task by clicking modify button on the task pop up
- the system should allow to view task by list of tasks and by pop up after clicking on the task

## Nonfunctional requirements
### Efficency
- The task should be added under 1 sec
- The user should be logged in under 1 sec
### Security
- The user should be allowed to allowed to manage only user's account
### Audit
- Errors should be displayed by logs
