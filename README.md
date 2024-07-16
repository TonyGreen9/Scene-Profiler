# Scene Profiler Tool for Unity

![Scene Profiler Tool](https://github.com/TonyGreen9/Scene-Profiler/raw/main/SceneProfiler.png)

The Scene Profiler Tool is a powerful utility designed to help Unity developers optimize their scenes by providing comprehensive profiling of various scene elements.

Supports Unity 2022.2 or higher

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Features](#features)
  - [Physics Profiler](#physics-profiler)
  - [Audio Clips Profiler](#audio-clips-profiler)
  - [Particle Systems Profiler](#particle-systems-profiler)
  - [Lights Profiler](#lights-profiler)
  - [Materials Profiler](#materials-profiler)
  - [Meshes Profiler](#meshes-profiler)
  - [Textures Profiler](#textures-profiler)
  - [Warnings Data Collector](#warnings-data-collector)
  - [Missing Assets Profiler](#missing-assets-profiler)
  - [Expensive Objects Profiler](#expensive-objects-profiler)
- [Modularity](#modularity)
- [Sorting and Multi-Column Usage](#sorting-and-multi-column-usage)
- [Usage](#usage)
- [Support](#support)
- [License](#license)

## Overview

The Scene Profiler Tool provides detailed insights into the performance of various elements within a Unity scene. It helps developers identify and resolve performance bottlenecks, ensuring a smooth and optimized gaming experience.

## Installation

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html).

## Getting Started

1. After importing the package, navigate to the Scene Profiler Tool window by selecting `Window > Analysis > Scene Profiler` from the Unity menu.
2. In the Scene Profiler Tool window, you can select different profiling options to gather data on various scene elements.

## Features

### Physics Profiler

Collects and displays data related to physics objects in the scene. It monitors performance metrics of physics interactions and helps optimize physical simulations.

### Audio Clips Profiler

Analyzes and profiles audio clips used in the scene. Provides detailed information on audio clip properties and usage statistics.

### Particle Systems Profiler

Gathers data on particle systems, offering insights into their performance and impact on the scene. Helps in optimizing particle effects for better performance.

### Lights Profiler

Collects and presents data on all light sources within the scene. Provides metrics to optimize lighting for improved performance and visual quality.

### Materials Profiler

Profiles materials used in the scene, displaying detailed information on material properties and usage. Aids in optimizing material configurations for better rendering performance.

### Meshes Profiler

Collects data on meshes within the scene. Offers insights into mesh properties and their impact on performance.

### Textures Profiler

Analyzes textures used in the scene, providing detailed information on texture properties and usage. Helps in optimizing texture usage for improved performance.

### Warnings Data Collector

Collects and displays warnings related to various aspects of the scene. Helps in identifying and addressing potential issues that may affect performance.

### Missing Assets Profiler

Identifies and reports missing assets within the scene. Ensures that all necessary assets are present and properly referenced.

### Expensive Objects Profiler

Profiles objects in the scene that are deemed "expensive" due to their scale, hierarchy depth, or component count. Helps in identifying objects that may be causing performance bottlenecks due to their complexity.

## Modularity

The Scene Profiler Tool is modular, allowing you to enable or disable specific profiling modules based on your needs. This flexibility ensures that you can focus on the aspects of your scene that are most critical for optimization. Each module can be toggled on or off from the tool's interface.

## Sorting and Multi-Column Usage

The Scene Profiler Tool supports sorting and multi-column usage to enhance the analysis of profiling data. You can sort data within each profiler module by various criteria, such as name, size, and performance metrics. Additionally, the tool uses multi-column layouts to display detailed information for each profiled object, making it easier to compare and analyze data.

## Usage

1. Open the Scene Profiler Tool window from the Unity menu: `Window > Analysis > Scene Profiler`.
2. Select the profiling category you want to analyze (e.g., Physics, Audio Clips, Particle Systems, etc.).
3. Click the `Collect Data` button to start collecting data.
4. Review the collected data in the tool's interface. Use the insights provided to optimize the respective elements of your scene.
5. To clear the collected data and refresh the interface, click the `Clear` button.
6. To access additional settings for the profiler, click the `Settings` button. Here, you can configure options such as including disabled objects, looking in sprite animations, and more.
7. Click the `Modules` button to toggle the display of different profiling modules.

## Support

If you encounter any issues or have any questions regarding the Scene Profiler Tool, please message me at [iamtonygreen@gmail.com](mailto:iamtonygreen@gmail.com).

## License

The Scene Profiler Tool is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International Public License. For more information, see the LICENSE file included in the package.

