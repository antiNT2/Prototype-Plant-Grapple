using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    Emile emile = new Emile();

    Employee[] allEmployees;

}

class Employee
{
    public int salary;
}

class Emile : Employee
{
    public Color colorOfHat;
}


class Workplace
{
    Employee[] employees;
    List<Employee> employeesList;
}

