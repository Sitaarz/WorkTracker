# MVP document for the project

## Goal
The goal of the project is to create an application for managing work with a
Kanban-style board.

## Problem
The project addresses the problem of organizing and tracking work.

## Target user
The target user is a person who wants to manage work through a task dashboard.

## MVP
- Add tasks
- Delete tasks
- Edit tasks
- Register
- Log in
- View tasks


## MoSCoW
Must have:
- Add tasks
- Delete tasks
- Edit tasks
- Register
- Log in
- View tasks

Should have:
- Labels

Could have:
- Sorting
- Filtering
- Notifications
- Comments
- Due dates
- Drag-and-drop

Will not have:
- Team collaboration
- Advanced reporting
- Mobile app


## User stories
- As a user, I want to log in.
- As a user, I want to register.
- As a user, I want to add tasks.
- As a user, I want to remove tasks.
- As a user, I want to modify tasks.
- As a user, I want to view my tasks.

## Functional requirements
- The system should allow users to log in with an email address and password.
- The system should allow users to register with an email address and password.
- The system should allow users to add tasks with a title and description.
- The system should allow users to remove tasks from the task details popup.
- The system should allow users to modify tasks from the task details popup.
- The system should allow users to view tasks in a task list and in a task details popup.

## Nonfunctional requirements
### Efficiency
- A task should be added within 1 second.
- A user should be logged in within 1 second.
### Security
- Users should be allowed to manage only their own account and tasks.
### Audit
- Errors should be recorded in logs.
