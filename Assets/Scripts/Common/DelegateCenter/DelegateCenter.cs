﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This could be replaced by a IoC framework like Zenject or something like a dependency
// Injection function.

// This is pretty much a simple service locator type of inversion of control mechanism.
// It manages seperately defined dependencies in one class by taking the advantage
// of the partial class. The dependencies can inject themselves into this service
// locator by defining their delegates and make themselves available to other components;
// They don't have to firmly depending on each other.
public partial class DelegateCenter : MonoInjectable {

}
