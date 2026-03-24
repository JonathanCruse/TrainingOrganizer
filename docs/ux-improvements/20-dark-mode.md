# 20 — Dark Mode

## Priority: Low

## Problem

The app has a single light theme with no option to switch. Many users prefer dark mode, especially when checking their training schedule in the evening or using the mobile app. MudBlazor has built-in dark mode support, making this relatively straightforward.

## Affected Roles

- **All users** — Personal preference for visual comfort

## Use Cases

### UC-1: User switches to dark mode

As a user, I want to toggle between light and dark mode based on my preference.

- In the top app bar, there's a theme toggle (sun/moon icon)
- Clicking it switches the entire app between light and dark themes
- All components, pages, and dialogs respect the theme

### UC-2: App remembers my preference

As a user, I want the app to remember my theme choice.

- After switching to dark mode, closing and reopening the app retains my choice
- Preference is stored in browser local storage (web) or app settings (mobile)

### UC-3: App defaults to system preference

As a user, I want the app to start with whatever theme my device/browser prefers.

- On first visit, the app checks the OS/browser dark mode preference
- If my system is set to dark mode, the app starts in dark mode
- I can override this with the manual toggle

## Acceptance Criteria

- Theme toggle button in the top app bar
- Full dark mode theme applied to all MudBlazor components
- Theme preference persisted across sessions
- Defaults to system preference on first visit
- All custom styles/colors adapt to the theme (no hard-coded colors breaking in dark mode)
- Works on both web and mobile
