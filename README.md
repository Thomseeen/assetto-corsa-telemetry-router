# Assetto Corsa Telemetry Router
Simple C# tool to send data from AC and ACC's shared memory to i.e. an Arduino via Serial COM port.

Feel free to check out my current approach to accessing the shared memory with my own lib here: https://github.com/Thomseeen/Thomsen.AccTools.

Uses mdjarv's nice shared memory library for Assetto Corsa https://github.com/mdjarv/assettocorsasharedmemory and Billiam's data format for sending to be compatible with devices set up for https://github.com/Billiam/pygauge.

Works in combination with i.e. https://github.com/Thomseeen/shift-indicator/tree/master.
