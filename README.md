# Seismiq Avalonia Exercise

## Instructions

We'd like to see how you'd design a basic UI application using Avalonia. This exercise is designed to take around an hour. Please fork this repo and then send us a message with a link to your fork when ready to submit.

We've provided a template application with model interfaces and implementations, we would like you to write the view models and views for the application.

The exact design of the UI is entirely up to you, feel free to be as creative as you wish. A good design should display the provided data in a graphically interesting way without sacrificing usability or functionality. We would like to see use of colour, icons, animations and motion (where appropriate!), etc. 

Use of AI coding tools is permitted, however you should be prepared to explain your design choices and code to us.

Don't be too concerned about squashing every single possible bug or possible visual glitch, this exercise will primarily be assessed on the design choices you make rather than the exact implementation.

You are free to use any external dependencies you wish providing they are available as from Nuget.

If anything is unclear or you have any questions or problems please ask us! The purpose of this exercise is for you to show off your design so we'd rather help you out with any technical problems than you submit something that doesn't demonstrate the best of your skills.

## Goals

The application is a monitoring app for a pedestrian crossing with traffic lights. The model will give you randomized generated data in real-time on pedestrians waiting to cross a road. Your application should display real-time information on all the pedestrians, the current status of the traffic lights, and also provide controls for the user to trigger the lights to allow them to cross the road.

## Models

All provided model interfaces and classes are within the 'Models' namespace. The 'Impl' namespace contains the actual implementations of the models which you can reference but should not code against.

## ViewModels

The template includes the CommunityToolkit.Mvvm library as a nuget dependency. (https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/). You 
should use base classes from this library as the base for your ViewModels.

## Threading

The provided model implementations are designed to be thread safe, however you should take into account that the events they produce may occur on threads other than the UI thread. See https://docs.avaloniaui.net/docs/app-development/threading for information on how to schedule execution on the UI thread in Avalonia.

## Styles

The template includes the Avalonia FluentTheme already configured, however feel free to change this to any other theme if you wish, or customize the control styles.

## Icons

The template includes the FontAwesome 6 Free icons library. You can use this via an included nuget dependency which provides custom Avalonia controls, for usage instructions see https://github.com/Projektanker/Icons.Avalonia.

You can use any of the icons listed here https://fontawesome.com/v6/search?ip=classic&ic=free-collection.